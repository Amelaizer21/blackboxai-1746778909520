from flask import Blueprint, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity
from sqlalchemy import func
from datetime import datetime, timedelta

from models.transaction import Transaction
from models.key import Key
from models.access_card import AccessCard
from models.department import Department
from models.employee import Employee

dashboard_bp = Blueprint('dashboard', __name__)

@dashboard_bp.route('/stats', methods=['GET'])
@jwt_required()
def get_dashboard_stats():
    """Get real-time dashboard statistics"""
    try:
        # Get current counts
        total_keys = Key.query.count()
        total_cards = AccessCard.query.count()
        total_employees = Employee.query.count()
        
        # Get active checkouts
        active_key_checkouts = Transaction.query.filter(
            Transaction.key_id.isnot(None),
            Transaction.check_in_time.is_(None)
        ).count()
        
        active_card_checkouts = Transaction.query.filter(
            Transaction.access_card_id.isnot(None),
            Transaction.check_in_time.is_(None)
        ).count()
        
        # Get overdue items
        now = datetime.utcnow()
        overdue_transactions = Transaction.query.filter(
            Transaction.check_in_time.is_(None),
            Transaction.expected_return_time < now
        ).all()
        
        overdue_items = [{
            'transaction_number': t.transaction_number,
            'employee_name': t.employee.full_name,
            'item_type': 'key' if t.key_id else 'access_card',
            'item_id': t.key.key_number if t.key_id else t.access_card.card_number,
            'checkout_time': t.check_out_time.isoformat(),
            'expected_return': t.expected_return_time.isoformat(),
            'hours_overdue': round((now - t.expected_return_time).total_seconds() / 3600, 1)
        } for t in overdue_transactions]
        
        # Get department-wise usage statistics
        dept_stats = get_department_stats()
        
        # Get recent activity
        recent_activity = get_recent_activity()
        
        # Get availability statistics
        availability_stats = {
            'keys': {
                'total': total_keys,
                'available': Key.query.filter_by(status='available').count(),
                'checked_out': active_key_checkouts,
                'lost': Key.query.filter_by(status='lost').count(),
                'retired': Key.query.filter_by(status='retired').count()
            },
            'access_cards': {
                'total': total_cards,
                'active': AccessCard.query.filter_by(status='active').count(),
                'inactive': AccessCard.query.filter_by(status='inactive').count(),
                'lost': AccessCard.query.filter_by(status='lost').count()
            }
        }
        
        return jsonify({
            'current_stats': {
                'total_keys': total_keys,
                'total_cards': total_cards,
                'total_employees': total_employees,
                'active_checkouts': {
                    'keys': active_key_checkouts,
                    'cards': active_card_checkouts
                }
            },
            'overdue_items': overdue_items,
            'department_stats': dept_stats,
            'recent_activity': recent_activity,
            'availability': availability_stats
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

def get_department_stats():
    """Get department-wise usage statistics"""
    # Get last 30 days of transactions
    thirty_days_ago = datetime.utcnow() - timedelta(days=30)
    
    dept_stats = db.session.query(
        Department.name,
        func.count(Transaction.id).label('total_transactions'),
        func.count(Transaction.key_id).label('key_checkouts'),
        func.count(Transaction.access_card_id).label('card_checkouts')
    ).join(
        Employee, Employee.department_id == Department.id
    ).join(
        Transaction, Transaction.employee_id == Employee.id
    ).filter(
        Transaction.check_out_time >= thirty_days_ago
    ).group_by(
        Department.name
    ).all()
    
    return [{
        'department': stat[0],
        'total_transactions': stat[1],
        'key_checkouts': stat[2],
        'card_checkouts': stat[3]
    } for stat in dept_stats]

def get_recent_activity():
    """Get recent transactions and activities"""
    recent_transactions = Transaction.query.order_by(
        Transaction.check_out_time.desc()
    ).limit(10).all()
    
    return [{
        'transaction_number': t.transaction_number,
        'timestamp': t.check_out_time.isoformat(),
        'employee_name': t.employee.full_name,
        'action': 'checked out' if not t.check_in_time else 'checked in',
        'item_type': 'key' if t.key_id else 'access_card',
        'item_id': t.key.key_number if t.key_id else t.access_card.card_number
    } for t in recent_transactions]

@dashboard_bp.route('/heatmap', methods=['GET'])
@jwt_required()
def get_usage_heatmap():
    """Get department-wise key/card usage heatmap data"""
    try:
        # Get transactions for the last 7 days
        seven_days_ago = datetime.utcnow() - timedelta(days=7)
        
        transactions = db.session.query(
            Department.name,
            func.date(Transaction.check_out_time),
            func.count(Transaction.id)
        ).join(
            Employee, Employee.department_id == Department.id
        ).join(
            Transaction, Transaction.employee_id == Employee.id
        ).filter(
            Transaction.check_out_time >= seven_days_ago
        ).group_by(
            Department.name,
            func.date(Transaction.check_out_time)
        ).all()
        
        # Format data for heatmap
        departments = list(set(t[0] for t in transactions))
        dates = list(set(t[1].isoformat() for t in transactions))
        
        heatmap_data = {
            'departments': departments,
            'dates': sorted(dates),
            'values': []
        }
        
        for dept in departments:
            dept_data = []
            for date in sorted(dates):
                count = next(
                    (t[2] for t in transactions 
                     if t[0] == dept and t[1].isoformat() == date),
                    0
                )
                dept_data.append(count)
            heatmap_data['values'].append(dept_data)
        
        return jsonify(heatmap_data), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@dashboard_bp.route('/alerts', methods=['GET'])
@jwt_required()
def get_alerts():
    """Get system alerts and notifications"""
    try:
        now = datetime.utcnow()
        alerts = []
        
        # Check for overdue items
        overdue_transactions = Transaction.query.filter(
            Transaction.check_in_time.is_(None),
            Transaction.expected_return_time < now
        ).all()
        
        for t in overdue_transactions:
            alerts.append({
                'type': 'overdue',
                'priority': 'high',
                'message': f"Overdue {t.key.key_number if t.key_id else t.access_card.card_number} "
                          f"checked out by {t.employee.full_name}",
                'timestamp': t.expected_return_time.isoformat()
            })
        
        # Check for expiring access cards
        soon = now + timedelta(days=7)
        expiring_cards = AccessCard.query.filter(
            AccessCard.expiry_date.between(now, soon)
        ).all()
        
        for card in expiring_cards:
            alerts.append({
                'type': 'expiring_card',
                'priority': 'medium',
                'message': f"Access card {card.card_number} will expire on "
                          f"{card.expiry_date.strftime('%Y-%m-%d')}",
                'timestamp': card.expiry_date.isoformat()
            })
        
        # Sort alerts by priority and timestamp
        alerts.sort(key=lambda x: (
            0 if x['priority'] == 'high' else 1 if x['priority'] == 'medium' else 2,
            x['timestamp']
        ))
        
        return jsonify(alerts), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500
