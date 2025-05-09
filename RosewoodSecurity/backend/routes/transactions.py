from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from datetime import datetime, timedelta

from app import db
from models.transaction import Transaction
from models.employee import Employee
from models.key import Key
from models.access_card import AccessCard
from utils.permissions import security_staff_required

transactions_bp = Blueprint('transactions', __name__)

@transactions_bp.route('/checkout', methods=['POST'])
@jwt_required()
@security_staff_required
def checkout_item():
    """Check out a key or access card to an employee"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['employee_id', 'purpose']
        for field in required_fields:
            if field not in data:
                return jsonify({'error': f'Missing required field: {field}'}), 400
        
        if not ('key_id' in data or 'access_card_id' in data):
            return jsonify({'error': 'Either key_id or access_card_id must be provided'}), 400
        
        if 'key_id' in data and 'access_card_id' in data:
            return jsonify({'error': 'Cannot checkout both key and access card simultaneously'}), 400
        
        # Verify employee exists and is active
        employee = Employee.query.get(data['employee_id'])
        if not employee:
            return jsonify({'error': 'Employee not found'}), 404
        if employee.status != 'active':
            return jsonify({'error': 'Employee is not active'}), 400
        
        # Handle key checkout
        if 'key_id' in data:
            key = Key.query.get(data['key_id'])
            if not key:
                return jsonify({'error': 'Key not found'}), 404
            
            # Check key availability
            if not key.is_available():
                return jsonify({'error': 'Key is not available for checkout'}), 400
            
            # Verify department permissions
            if not employee.can_checkout_key(key.id):
                return jsonify({'error': 'Employee department does not have permission for this key'}), 403
            
            item_id = key.id
            item_type = 'key'
            
        # Handle access card checkout
        else:
            card = AccessCard.query.get(data['access_card_id'])
            if not card:
                return jsonify({'error': 'Access card not found'}), 404
            
            # Check card status
            if card.status != 'active':
                return jsonify({'error': 'Access card is not active'}), 400
            
            if card.is_expired():
                return jsonify({'error': 'Access card has expired'}), 400
            
            item_id = card.id
            item_type = 'access_card'
        
        # Calculate expected return time
        expected_return_time = None
        if data.get('expected_return_hours'):
            expected_return_time = datetime.utcnow() + timedelta(
                hours=float(data['expected_return_hours'])
            )
        
        # Create transaction
        transaction = Transaction(
            employee_id=employee.id,
            key_id=item_id if item_type == 'key' else None,
            access_card_id=item_id if item_type == 'access_card' else None,
            purpose=data['purpose'],
            expected_return_time=expected_return_time,
            created_by=get_jwt_identity()
        )
        
        db.session.add(transaction)
        
        # Update item status
        if item_type == 'key':
            key.check_out(employee.id)
        else:
            card.record_usage()
        
        db.session.commit()
        
        return jsonify({
            'message': f'{item_type.capitalize()} checked out successfully',
            'transaction': transaction.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/checkin', methods=['POST'])
@jwt_required()
@security_staff_required
def checkin_item():
    """Check in a previously checked out key or access card"""
    try:
        data = request.get_json()
        
        if 'transaction_number' not in data:
            return jsonify({'error': 'Transaction number is required'}), 400
        
        # Find transaction
        transaction = Transaction.query.filter_by(
            transaction_number=data['transaction_number']
        ).first()
        
        if not transaction:
            return jsonify({'error': 'Transaction not found'}), 404
        
        if transaction.check_in_time:
            return jsonify({'error': 'Item has already been checked in'}), 400
        
        # Process check-in
        transaction.check_in(notes=data.get('notes'))
        
        return jsonify({
            'message': 'Item checked in successfully',
            'transaction': transaction.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/active', methods=['GET'])
@jwt_required()
def get_active_transactions():
    """Get all active (checked out) transactions"""
    try:
        # Get query parameters
        employee_id = request.args.get('employee_id')
        item_type = request.args.get('item_type')  # 'key' or 'access_card'
        
        # Base query for active transactions
        query = Transaction.query.filter(Transaction.check_in_time.is_(None))
        
        # Apply filters
        if employee_id:
            query = query.filter_by(employee_id=employee_id)
        if item_type == 'key':
            query = query.filter(Transaction.key_id.isnot(None))
        elif item_type == 'access_card':
            query = query.filter(Transaction.access_card_id.isnot(None))
        
        active_transactions = query.all()
        
        return jsonify({
            'active_transactions': [t.to_dict() for t in active_transactions]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/overdue', methods=['GET'])
@jwt_required()
def get_overdue_transactions():
    """Get all overdue transactions"""
    try:
        now = datetime.utcnow()
        
        # Find all transactions that are past their expected return time
        overdue = Transaction.query.filter(
            Transaction.check_in_time.is_(None),
            Transaction.expected_return_time < now
        ).all()
        
        return jsonify({
            'overdue_transactions': [t.to_dict() for t in overdue]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/<transaction_number>', methods=['GET'])
@jwt_required()
def get_transaction(transaction_number):
    """Get details of a specific transaction"""
    try:
        transaction = Transaction.query.filter_by(
            transaction_number=transaction_number
        ).first_or_404()
        
        return jsonify(transaction.to_dict()), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/<transaction_number>/extend', methods=['POST'])
@jwt_required()
@security_staff_required
def extend_checkout(transaction_number):
    """Extend the expected return time for a checked out item"""
    try:
        data = request.get_json()
        
        if 'additional_hours' not in data:
            return jsonify({'error': 'Additional hours required'}), 400
        
        transaction = Transaction.query.filter_by(
            transaction_number=transaction_number
        ).first_or_404()
        
        if transaction.check_in_time:
            return jsonify({'error': 'Cannot extend checked-in transaction'}), 400
        
        # Calculate new return time
        additional_hours = float(data['additional_hours'])
        if transaction.expected_return_time:
            new_return_time = transaction.expected_return_time + timedelta(hours=additional_hours)
        else:
            new_return_time = datetime.utcnow() + timedelta(hours=additional_hours)
        
        transaction.update_expected_return(new_return_time)
        
        return jsonify({
            'message': 'Checkout period extended successfully',
            'transaction': transaction.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@transactions_bp.route('/<transaction_number>/lost', methods=['POST'])
@jwt_required()
@security_staff_required
def report_lost(transaction_number):
    """Report a checked out item as lost"""
    try:
        data = request.get_json()
        
        transaction = Transaction.query.filter_by(
            transaction_number=transaction_number
        ).first_or_404()
        
        if transaction.check_in_time:
            return jsonify({'error': 'Cannot mark checked-in item as lost'}), 400
        
        transaction.mark_lost(notes=data.get('notes'))
        
        return jsonify({
            'message': 'Item marked as lost',
            'transaction': transaction.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500
