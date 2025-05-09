from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from datetime import datetime
import re

from app import db
from models.employee import Employee
from models.department import Department
from models.transaction import Transaction
from utils.permissions import admin_required

employees_bp = Blueprint('employees', __name__)

@employees_bp.route('/', methods=['GET'])
@jwt_required()
def get_employees():
    """Get list of all employees with optional filters"""
    try:
        # Get query parameters
        department_id = request.args.get('department_id')
        status = request.args.get('status')
        search = request.args.get('search')
        
        # Base query
        query = Employee.query
        
        # Apply filters
        if department_id:
            query = query.filter(Employee.department_id == department_id)
        if status:
            query = query.filter(Employee.status == status)
        if search:
            search_term = f"%{search}%"
            query = query.filter(
                db.or_(
                    Employee.first_name.ilike(search_term),
                    Employee.last_name.ilike(search_term),
                    Employee.employee_number.ilike(search_term),
                    Employee.email.ilike(search_term)
                )
            )
        
        # Execute query
        employees = query.all()
        
        return jsonify({
            'employees': [emp.to_dict() for emp in employees]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/<int:employee_id>', methods=['GET'])
@jwt_required()
def get_employee(employee_id):
    """Get details of a specific employee"""
    try:
        employee = Employee.query.get_or_404(employee_id)
        return jsonify(employee.to_dict()), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/', methods=['POST'])
@jwt_required()
@admin_required
def create_employee():
    """Create a new employee"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['employee_number', 'first_name', 'last_name', 
                         'email', 'department_id']
        for field in required_fields:
            if field not in data:
                return jsonify({'error': f'Missing required field: {field}'}), 400
        
        # Validate email format
        if not re.match(r"[^@]+@[^@]+\.[^@]+", data['email']):
            return jsonify({'error': 'Invalid email format'}), 400
        
        # Check for duplicate employee number or email
        if Employee.query.filter_by(employee_number=data['employee_number']).first():
            return jsonify({'error': 'Employee number already exists'}), 400
        if Employee.query.filter_by(email=data['email']).first():
            return jsonify({'error': 'Email already exists'}), 400
        
        # Verify department exists
        department = Department.query.get(data['department_id'])
        if not department:
            return jsonify({'error': 'Department not found'}), 404
        
        # Create new employee
        employee = Employee(
            employee_number=data['employee_number'],
            first_name=data['first_name'],
            last_name=data['last_name'],
            email=data['email'],
            department_id=data['department_id'],
            phone=data.get('phone'),
            photo_url=data.get('photo_url')
        )
        
        db.session.add(employee)
        db.session.commit()
        
        return jsonify({
            'message': 'Employee created successfully',
            'employee': employee.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/<int:employee_id>', methods=['PUT'])
@jwt_required()
@admin_required
def update_employee(employee_id):
    """Update an existing employee"""
    try:
        employee = Employee.query.get_or_404(employee_id)
        data = request.get_json()
        
        # Update basic fields
        if 'first_name' in data:
            employee.first_name = data['first_name']
        if 'last_name' in data:
            employee.last_name = data['last_name']
        if 'phone' in data:
            employee.phone = data['phone']
        if 'photo_url' in data:
            employee.photo_url = data['photo_url']
        
        # Update email if provided and unique
        if 'email' in data and data['email'] != employee.email:
            if not re.match(r"[^@]+@[^@]+\.[^@]+", data['email']):
                return jsonify({'error': 'Invalid email format'}), 400
            if Employee.query.filter_by(email=data['email']).first():
                return jsonify({'error': 'Email already exists'}), 400
            employee.email = data['email']
        
        # Update department if provided
        if 'department_id' in data:
            department = Department.query.get(data['department_id'])
            if not department:
                return jsonify({'error': 'Department not found'}), 404
            employee.department_id = data['department_id']
        
        # Update status if provided
        if 'status' in data:
            if data['status'] not in ['active', 'inactive', 'suspended']:
                return jsonify({'error': 'Invalid status'}), 400
            employee.status = data['status']
        
        db.session.commit()
        
        return jsonify({
            'message': 'Employee updated successfully',
            'employee': employee.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/<int:employee_id>/transactions', methods=['GET'])
@jwt_required()
def get_employee_transactions(employee_id):
    """Get transaction history for an employee"""
    try:
        employee = Employee.query.get_or_404(employee_id)
        
        # Get query parameters
        status = request.args.get('status')
        item_type = request.args.get('item_type')  # 'key' or 'access_card'
        
        # Base query
        query = Transaction.query.filter_by(employee_id=employee_id)
        
        # Apply filters
        if status:
            query = query.filter_by(status=status)
        if item_type == 'key':
            query = query.filter(Transaction.key_id.isnot(None))
        elif item_type == 'access_card':
            query = query.filter(Transaction.access_card_id.isnot(None))
        
        transactions = query.order_by(Transaction.check_out_time.desc()).all()
        
        return jsonify({
            'employee': employee.full_name,
            'transactions': [t.to_dict() for t in transactions]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/<int:employee_id>/active-items', methods=['GET'])
@jwt_required()
def get_employee_active_items(employee_id):
    """Get currently checked out items for an employee"""
    try:
        employee = Employee.query.get_or_404(employee_id)
        active_items = employee.get_active_checkouts()
        
        return jsonify({
            'employee': employee.full_name,
            'active_items': active_items
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

# Department routes
@employees_bp.route('/departments', methods=['GET'])
@jwt_required()
def get_departments():
    """Get list of all departments"""
    try:
        departments = Department.query.all()
        return jsonify({
            'departments': [dept.to_dict() for dept in departments]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/departments', methods=['POST'])
@jwt_required()
@admin_required
def create_department():
    """Create a new department"""
    try:
        data = request.get_json()
        
        if 'name' not in data:
            return jsonify({'error': 'Department name is required'}), 400
        
        # Check for duplicate department name
        if Department.query.filter_by(name=data['name']).first():
            return jsonify({'error': 'Department name already exists'}), 400
        
        department = Department(
            name=data['name'],
            description=data.get('description'),
            access_level=data.get('access_level', 1)
        )
        
        db.session.add(department)
        db.session.commit()
        
        return jsonify({
            'message': 'Department created successfully',
            'department': department.to_dict()
        }), 201
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/departments/<int:dept_id>', methods=['PUT'])
@jwt_required()
@admin_required
def update_department(dept_id):
    """Update an existing department"""
    try:
        department = Department.query.get_or_404(dept_id)
        data = request.get_json()
        
        if 'name' in data and data['name'] != department.name:
            if Department.query.filter_by(name=data['name']).first():
                return jsonify({'error': 'Department name already exists'}), 400
            department.name = data['name']
            
        if 'description' in data:
            department.description = data['description']
        if 'access_level' in data:
            department.access_level = data['access_level']
        
        db.session.commit()
        
        return jsonify({
            'message': 'Department updated successfully',
            'department': department.to_dict()
        }), 200
        
    except Exception as e:
        db.session.rollback()
        return jsonify({'error': str(e)}), 500

@employees_bp.route('/departments/<int:dept_id>/employees', methods=['GET'])
@jwt_required()
def get_department_employees(dept_id):
    """Get all employees in a department"""
    try:
        department = Department.query.get_or_404(dept_id)
        return jsonify({
            'department': department.name,
            'employees': [emp.to_dict() for emp in department.employees]
        }), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500
