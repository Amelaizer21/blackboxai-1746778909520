from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from datetime import datetime, timedelta

from app import db
from models.access_card import AccessCard
from models.employee import Employee
from models.department import Department
from utils.permissions import admin_required

access_cards_bp = Blueprint('access_cards', __name__)

@access_cards_bp.route('/', methods=['GET'])
@jwt_required()
def get_access_cards():
    """Get list of all access cards with optional filters"""
    try:
        # Get query parameters
        status = request.args.get('status')
        card_type = request.args.get('card_type')
        department_id = request.args.get('department_id')
        include_expired = request.args.get('include_expired', 'false').lower() == 'true'
        
        # Base query
        query = AccessCard.query
        
        # Apply filters
        if status:
            query = query.filter(AccessCard.status == status)
        if card_type:
            query = query.filter(AccessCard.card_type == card_type)
        if department_id:
            query = query.join(AccessCard.assigned_employee).filter(
                Employee.department_id == department_id
            )
        if not include_expired:
            query = query.filter(
                (AccessCard.expiry_date > datetime.utcnow()) | 
                (AccessCard.expiry_date.is_(None))
            )
        
        # Execute query
        cards = query.all()
        
        return jsonify({
            'access_cards': [card.to_dict() for card in cards]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/<int:card_id>', methods=['GET'])
@jwt_required()
def get_access_card(card_id):
    """Get details of a specific access card"""
    try:
        card = AccessCard.query.get_or_404(card_id)
        return jsonify(card.to_dict()), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/', methods=['POST'])
@jwt_required()
@admin_required
def create_access_card():
    """Create a new access card"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['card_number', 'card_type', 'access_zones']
        for field in required_fields:
            if field not in data:
                return jsonify({'error': f'Missing required field: {field}'}), 400
        
        # Check for duplicate card number
        if AccessCard.query.filter_by(card_number=data['card_number']).first():
            return jsonify({'error': 'Card number already exists'}), 400
        
        # Parse expiry date if provided
        expiry_date = None
        if data.get('expiry_date'):
            try:
                expiry_date = datetime.fromisoformat(data['expiry_date'])
            except ValueError:
                return jsonify({'error': 'Invalid expiry date format'}), 400
        
        # Create new access card
        card = AccessCard(
            card_number=data['card_number'],
            card_type=data['card_type'],
            access_zones=data['access_zones'],
            employee_id=data.get('employee_id'),
            expiry_date=expiry_date
        )
        
        db.session.add(card)
        db.session.commit()
        
        return jsonify({
            'message': 'Access card created successfully',
            'access_card': card.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/<int:card_id>', methods=['PUT'])
@jwt_required()
@admin_required
def update_access_card(card_id):
    """Update an existing access card"""
    try:
        card = AccessCard.query.get_or_404(card_id)
        data = request.get_json()
        
        # Update basic fields
        if 'card_type' in data:
            card.card_type = data['card_type']
        if 'access_zones' in data:
            card.access_zones = data['access_zones']
        if 'status' in data:
            card.status = data['status']
        
        # Update expiry date if provided
        if 'expiry_date' in data:
            try:
                card.expiry_date = datetime.fromisoformat(data['expiry_date'])
            except ValueError:
                return jsonify({'error': 'Invalid expiry date format'}), 400
        
        # Update employee assignment
        if 'employee_id' in data:
            if data['employee_id'] is None:
                card.unassign()
            else:
                employee = Employee.query.get(data['employee_id'])
                if not employee:
                    return jsonify({'error': 'Employee not found'}), 404
                card.assign_to_employee(employee.id)
        
        db.session.commit()
        
        return jsonify({
            'message': 'Access card updated successfully',
            'access_card': card.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/<int:card_id>', methods=['DELETE'])
@jwt_required()
@admin_required
def delete_access_card(card_id):
    """Deactivate an access card"""
    try:
        card = AccessCard.query.get_or_404(card_id)
        
        # Check if card has any active checkouts
        if any(t for t in card.transactions if not t.check_in_time):
            return jsonify({
                'error': 'Cannot delete card with active checkouts'
            }), 400
        
        # Deactivate the card
        card.deactivate()
        
        return jsonify({
            'message': 'Access card deactivated successfully'
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/<int:card_id>/extend', methods=['POST'])
@jwt_required()
@admin_required
def extend_card_expiry(card_id):
    """Extend the expiry date of an access card"""
    try:
        card = AccessCard.query.get_or_404(card_id)
        data = request.get_json()
        
        if 'new_expiry_date' not in data:
            return jsonify({'error': 'New expiry date is required'}), 400
            
        try:
            new_expiry_date = datetime.fromisoformat(data['new_expiry_date'])
        except ValueError:
            return jsonify({'error': 'Invalid expiry date format'}), 400
            
        card.extend_expiry(new_expiry_date)
        
        return jsonify({
            'message': 'Access card expiry extended successfully',
            'new_expiry_date': new_expiry_date.isoformat()
        }), 200
        
    except ValueError as e:
        return jsonify({'error': str(e)}), 400
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/<int:card_id>/history', methods=['GET'])
@jwt_required()
def get_card_history(card_id):
    """Get usage history for an access card"""
    try:
        card = AccessCard.query.get_or_404(card_id)
        
        # Get all transactions for this card
        transactions = [{
            'transaction_number': t.transaction_number,
            'employee': t.employee.full_name,
            'check_out_time': t.check_out_time.isoformat(),
            'check_in_time': t.check_in_time.isoformat() if t.check_in_time else None,
            'purpose': t.purpose,
            'status': t.status
        } for t in card.transactions]
        
        return jsonify({
            'card_number': card.card_number,
            'transactions': transactions
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@access_cards_bp.route('/expiring', methods=['GET'])
@jwt_required()
def get_expiring_cards():
    """Get list of cards expiring soon"""
    try:
        days = int(request.args.get('days', 30))
        expiry_date = datetime.utcnow() + timedelta(days=days)
        
        cards = AccessCard.query.filter(
            AccessCard.expiry_date <= expiry_date,
            AccessCard.expiry_date > datetime.utcnow(),
            AccessCard.status == 'active'
        ).all()
        
        return jsonify({
            'expiring_cards': [
                {
                    **card.to_dict(),
                    'days_until_expiry': (card.expiry_date - datetime.utcnow()).days
                }
                for card in cards
            ]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500
