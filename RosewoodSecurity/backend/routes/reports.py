from flask import Blueprint, jsonify, send_file
from flask_jwt_extended import jwt_required, get_jwt_identity
from datetime import datetime, timedelta
import pandas as pd
from io import BytesIO
import os
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph
from reportlab.lib.styles import getSampleStyleSheet

from models.transaction import Transaction
from models.key import Key
from models.access_card import AccessCard
from models.employee import Employee
from models.department import Department
from utils.permissions import admin_required, auditor_required

reports_bp = Blueprint('reports', __name__)

@reports_bp.route('/daily', methods=['GET'])
@jwt_required()
@auditor_required
def generate_daily_report():
    """Generate a daily transaction report"""
    try:
        # Get yesterday's date
        yesterday = datetime.utcnow().date() - timedelta(days=1)
        start_date = datetime.combine(yesterday, datetime.min.time())
        end_date = datetime.combine(yesterday, datetime.max.time())
        
        # Get all transactions for yesterday
        transactions = Transaction.query.filter(
            Transaction.check_out_time.between(start_date, end_date)
        ).all()
        
        # Prepare data for report
        data = []
        for t in transactions:
            data.append({
                'Transaction Number': t.transaction_number,
                'Employee': t.employee.full_name,
                'Department': t.employee.department.name,
                'Item Type': 'Key' if t.key_id else 'Access Card',
                'Item ID': t.key.key_number if t.key_id else t.access_card.card_number,
                'Check Out Time': t.check_out_time.strftime('%Y-%m-%d %H:%M'),
                'Check In Time': t.check_in_time.strftime('%Y-%m-%d %H:%M') if t.check_in_time else 'Not Checked In',
                'Status': t.status
            })
        
        # Generate Excel report
        df = pd.DataFrame(data)
        excel_buffer = BytesIO()
        with pd.ExcelWriter(excel_buffer, engine='openpyxl') as writer:
            df.to_excel(writer, sheet_name='Daily Transactions', index=False)
        
        excel_buffer.seek(0)
        
        return send_file(
            excel_buffer,
            mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            as_attachment=True,
            download_name=f'daily_report_{yesterday.strftime("%Y-%m-%d")}.xlsx'
        )
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@reports_bp.route('/weekly', methods=['GET'])
@jwt_required()
@auditor_required
def generate_weekly_report():
    """Generate a weekly summary report"""
    try:
        # Get dates for last week
        end_date = datetime.utcnow().date() - timedelta(days=1)
        start_date = end_date - timedelta(days=6)
        
        # Generate PDF report
        buffer = BytesIO()
        doc = SimpleDocTemplate(buffer, pagesize=letter)
        styles = getSampleStyleSheet()
        elements = []
        
        # Add title
        title = Paragraph(f"Weekly Security Report ({start_date} to {end_date})", 
                         styles['Heading1'])
        elements.append(title)
        
        # Add transaction summary
        transactions = Transaction.query.filter(
            Transaction.check_out_time.between(
                datetime.combine(start_date, datetime.min.time()),
                datetime.combine(end_date, datetime.max.time())
            )
        ).all()
        
        # Department summary
        dept_summary = {}
        for t in transactions:
            dept = t.employee.department.name
            if dept not in dept_summary:
                dept_summary[dept] = {'keys': 0, 'cards': 0}
            if t.key_id:
                dept_summary[dept]['keys'] += 1
            else:
                dept_summary[dept]['cards'] += 1
        
        dept_data = [['Department', 'Keys Checked Out', 'Cards Checked Out']]
        for dept, counts in dept_summary.items():
            dept_data.append([dept, counts['keys'], counts['cards']])
        
        dept_table = Table(dept_data)
        dept_table.setStyle(TableStyle([
            ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, 0), 14),
            ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
            ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
            ('TEXTCOLOR', (0, 1), (-1, -1), colors.black),
            ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
            ('FONTSIZE', (0, 1), (-1, -1), 12),
            ('GRID', (0, 0), (-1, -1), 1, colors.black)
        ]))
        elements.append(dept_table)
        
        # Add overdue items section
        elements.append(Paragraph("Overdue Items", styles['Heading2']))
        overdue = [t for t in transactions if t.is_overdue()]
        if overdue:
            overdue_data = [['Item', 'Employee', 'Department', 'Days Overdue']]
            for t in overdue:
                item_id = t.key.key_number if t.key_id else t.access_card.card_number
                days_overdue = (datetime.utcnow() - t.expected_return_time).days
                overdue_data.append([
                    item_id,
                    t.employee.full_name,
                    t.employee.department.name,
                    days_overdue
                ])
            
            overdue_table = Table(overdue_data)
            overdue_table.setStyle(TableStyle([
                ('BACKGROUND', (0, 0), (-1, 0), colors.red),
                ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
                ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
                ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
                ('GRID', (0, 0), (-1, -1), 1, colors.black)
            ]))
            elements.append(overdue_table)
        else:
            elements.append(Paragraph("No overdue items", styles['Normal']))
        
        # Build PDF
        doc.build(elements)
        buffer.seek(0)
        
        return send_file(
            buffer,
            mimetype='application/pdf',
            as_attachment=True,
            download_name=f'weekly_report_{end_date}.pdf'
        )
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@reports_bp.route('/audit-trail', methods=['GET'])
@jwt_required()
@admin_required
def get_audit_trail():
    """Get system audit trail"""
    try:
        # Get query parameters
        start_date = request.args.get('start_date')
        end_date = request.args.get('end_date')
        action_type = request.args.get('action_type')
        
        # Base query
        query = AuditLog.query
        
        # Apply filters
        if start_date:
            query = query.filter(AuditLog.timestamp >= datetime.fromisoformat(start_date))
        if end_date:
            query = query.filter(AuditLog.timestamp <= datetime.fromisoformat(end_date))
        if action_type:
            query = query.filter(AuditLog.action_type == action_type)
        
        # Execute query
        logs = query.order_by(AuditLog.timestamp.desc()).all()
        
        return jsonify({
            'audit_trail': [log.to_dict() for log in logs]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@reports_bp.route('/department/<int:dept_id>', methods=['GET'])
@jwt_required()
@auditor_required
def generate_department_report(dept_id):
    """Generate a department-specific report"""
    try:
        department = Department.query.get_or_404(dept_id)
        
        # Get all active employees in department
        employees = Employee.query.filter_by(
            department_id=dept_id,
            status='active'
        ).all()
        
        # Get department's key permissions
        authorized_keys = department.key_permissions
        
        # Get recent transactions
        recent_transactions = Transaction.query.join(
            Employee
        ).filter(
            Employee.department_id == dept_id
        ).order_by(
            Transaction.check_out_time.desc()
        ).limit(50).all()
        
        # Prepare report data
        report_data = {
            'department_name': department.name,
            'employee_count': len(employees),
            'key_permissions': [key.to_dict() for key in authorized_keys],
            'recent_transactions': [t.to_dict() for t in recent_transactions],
            'active_checkouts': [
                t.to_dict() for t in recent_transactions 
                if not t.check_in_time
            ]
        }
        
        return jsonify(report_data), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@reports_bp.route('/lost-items', methods=['GET'])
@jwt_required()
@auditor_required
def get_lost_items_report():
    """Generate a report of all lost items"""
    try:
        # Get lost keys
        lost_keys = Key.query.filter_by(status='lost').all()
        
        # Get lost access cards
        lost_cards = AccessCard.query.filter_by(status='lost').all()
        
        # Get transactions where items were reported lost
        lost_transactions = Transaction.query.filter_by(status='lost').all()
        
        report_data = {
            'lost_keys': [
                {
                    **key.to_dict(),
                    'last_transaction': next(
                        (t.to_dict() for t in key.transactions),
                        None
                    )
                }
                for key in lost_keys
            ],
            'lost_cards': [
                {
                    **card.to_dict(),
                    'last_transaction': next(
                        (t.to_dict() for t in card.transactions),
                        None
                    )
                }
                for card in lost_cards
            ],
            'lost_item_transactions': [t.to_dict() for t in lost_transactions]
        }
        
        return jsonify(report_data), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@reports_bp.route('/employee/<int:employee_id>', methods=['GET'])
@jwt_required()
@auditor_required
def generate_employee_report(employee_id):
    """Generate a detailed report for a specific employee"""
    try:
        employee = Employee.query.get_or_404(employee_id)
        
        # Get all transactions
        transactions = Transaction.query.filter_by(
            employee_id=employee_id
        ).order_by(
            Transaction.check_out_time.desc()
        ).all()
        
        # Calculate statistics
        total_transactions = len(transactions)
        active_checkouts = len([t for t in transactions if not t.check_in_time])
        overdue_items = len([t for t in transactions if t.is_overdue()])
        avg_checkout_duration = sum(
            t.get_duration() for t in transactions if t.check_in_time
        ) / len([t for t in transactions if t.check_in_time]) if transactions else 0
        
        report_data = {
            'employee': employee.to_dict(),
            'statistics': {
                'total_transactions': total_transactions,
                'active_checkouts': active_checkouts,
                'overdue_items': overdue_items,
                'avg_checkout_duration': round(avg_checkout_duration, 2)
            },
            'recent_transactions': [t.to_dict() for t in transactions[:10]],
            'department_permissions': [
                key.to_dict() for key in employee.department.key_permissions
            ]
        }
        
        return jsonify(report_data), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500
