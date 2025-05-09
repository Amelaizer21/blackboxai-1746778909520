from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from datetime import datetime
import uuid

from app import db
from models.key import Key
from models.user import User
from models.department import Department
from utils.permissions import admin_required

keys_bp = Blueprint('keys', __name__)

@keys_bp.route('/', methods=['GET'])
@jwt_required()
def get_keys():
    """Get list of all keys with optional filters"""
    try:
        # Get query parameters
        status = request.args.get('status')
        location = request.args.get('location')
        department_id = request.args.get('department_id')
        
        # Base query
        query = Key.query
        
        # Apply filters
        if status:
            query = query.filter(Key.status == status)
        if location:
            query = query.filter(Key.location.ilike(f'%{location}%'))
        if department_id:
            query = query.join(Key.authorized_departments).filter(
                Department.id == department_id
            )
        
        # Execute query
        keys = query.all()
        
        return jsonify({
            'keys': [key.to_dict() for key in keys]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>', methods=['GET'])
@jwt_required()
def get_key(key_id):
    """Get details of a specific key"""
    try:
        key = Key.query.get_or_404(key_id)
        return jsonify(key.to_dict()), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/', methods=['POST'])
@jwt_required()
@admin_required
def create_key():
    """Create a new key"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['key_number', 'name', 'location']
        for field in required_fields:
            if field not in data:
                return jsonify({'error': f'Missing required field: {field}'}), 400
        
        # Check for duplicate key number
        if Key.query.filter_by(key_number=data['key_number']).first():
            return jsonify({'error': 'Key number already exists'}), 400
        
        # Create new key
        key = Key(
            key_number=data['key_number'],
            name=data['name'],
            location=data['location'],
            description=data.get('description'),
            key_type=data.get('key_type', 'regular'),
            photo_url=data.get('photo_url')
        )
        
        # Add department permissions if provided
        if 'department_ids' in data:
            departments = Department.query.filter(
                Department.id.in_(data['department_ids'])
            ).all()
            key.authorized_departments.extend(departments)
        
        db.session.add(key)
        db.session.commit()
        
        return jsonify({
            'message': 'Key created successfully',
            'key': key.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>', methods=['PUT'])
@jwt_required()
@admin_required
def update_key(key_id):
    """Update an existing key"""
    try:
        key = Key.query.get_or_404(key_id)
        data = request.get_json()
        
        # Update basic fields
        updateable_fields = [
            'name', 'location', 'description', 'key_type',
            'photo_url', 'status'
        ]
        
        for field in updateable_fields:
            if field in data:
                setattr(key, field, data[field])
        
        # Update department permissions if provided
        if 'department_ids' in data:
            departments = Department.query.filter(
                Department.id.in_(data['department_ids'])
            ).all()
            key.authorized_departments = departments
        
        # Record maintenance if specified
        if data.get('maintenance_performed'):
            key.record_maintenance()
        
        db.session.commit()
        
        return jsonify({
            'message': 'Key updated successfully',
            'key': key.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>', methods=['DELETE'])
@jwt_required()
@admin_required
def delete_key(key_id):
    """Delete/retire a key"""
    try:
        key = Key.query.get_or_404(key_id)
        
        # Check if key has any active checkouts
        if any(t for t in key.transactions if not t.check_in_time):
            return jsonify({
                'error': 'Cannot delete key with active checkouts'
            }), 400
        
        # Soft delete by marking as retired
        key.retire()
        
        return jsonify({
            'message': 'Key retired successfully'
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>/permissions', methods=['GET'])
@jwt_required()
def get_key_permissions(key_id):
    """Get departments authorized to use this key"""
    try:
        key = Key.query.get_or_404(key_id)
        
        return jsonify({
            'departments': [
                {
                    'id': dept.id,
                    'name': dept.name,
                    'access_level': dept.access_level
                }
                for dept in key.authorized_departments
            ]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>/history', methods=['GET'])
@jwt_required()
def get_key_history(key_id):
    """Get checkout history for a key"""
    try:
        key = Key.query.get_or_404(key_id)
        
        # Get all transactions for this key
        transactions = [{
            'transaction_number': t.transaction_number,
            'employee': t.employee.full_name,
            'check_out_time': t.check_out_time.isoformat(),
            'check_in_time': t.check_in_time.isoformat() if t.check_in_time else None,
            'purpose': t.purpose,
            'status': t.status
        } for t in key.transactions]
        
        return jsonify({
            'key_number': key.key_number,
            'transactions': transactions
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@keys_bp.route('/<int:key_id>/maintenance', methods=['POST'])
@jwt_required()
@admin_required
def record_maintenance(key_id):
    """Record maintenance performed on a key"""
    try:
        key = Key.query.get_or_404(key_id)
        data = request.get_json()
        
        key.record_maintenance()
        
        # Add maintenance notes if provided
        if data.get('notes'):
            maintenance_record = {
                'date': datetime.utcnow().isoformat(),
                'performed_by': get_jwt_identity(),
                'notes': data['notes']
            }
            # In a real application, you might want to store this in a separate maintenance_logs table
        
        return jsonify({
            'message': 'Maintenance recorded successfully',
            'last_maintenance': key.last_maintenance.isoformat()
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500
