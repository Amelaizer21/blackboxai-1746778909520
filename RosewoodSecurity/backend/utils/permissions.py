from functools import wraps
from flask import jsonify
from flask_jwt_extended import get_jwt, verify_jwt_in_request

def admin_required(fn):
    """Decorator to require admin role for access"""
    @wraps(fn)
    def wrapper(*args, **kwargs):
        verify_jwt_in_request()
        claims = get_jwt()
        if claims.get('role') != 'admin':
            return jsonify({
                'error': 'Admin privileges required'
            }), 403
        return fn(*args, **kwargs)
    return wrapper

def security_staff_required(fn):
    """Decorator to require security staff role or higher for access"""
    @wraps(fn)
    def wrapper(*args, **kwargs):
        verify_jwt_in_request()
        claims = get_jwt()
        if claims.get('role') not in ['admin', 'security_staff']:
            return jsonify({
                'error': 'Security staff privileges required'
            }), 403
        return fn(*args, **kwargs)
    return wrapper

def auditor_required(fn):
    """Decorator to require auditor role or higher for access"""
    @wraps(fn)
    def wrapper(*args, **kwargs):
        verify_jwt_in_request()
        claims = get_jwt()
        if claims.get('role') not in ['admin', 'security_staff', 'auditor']:
            return jsonify({
                'error': 'Auditor privileges required'
            }), 403
        return fn(*args, **kwargs)
    return wrapper

def has_permission(required_role):
    """Check if current user has the required role permission"""
    claims = get_jwt()
    role_hierarchy = {
        'admin': ['admin', 'security_staff', 'auditor'],
        'security_staff': ['security_staff', 'auditor'],
        'auditor': ['auditor']
    }
    return required_role in role_hierarchy.get(claims.get('role', ''), [])

def validate_department_access(user_dept_id, target_dept_id):
    """Validate if user has access to target department"""
    claims = get_jwt()
    if claims.get('role') == 'admin':
        return True
    return user_dept_id == target_dept_id

def can_manage_keys(user_role):
    """Check if user role can manage keys"""
    return user_role in ['admin', 'security_staff']

def can_view_reports(user_role):
    """Check if user role can view reports"""
    return user_role in ['admin', 'security_staff', 'auditor']

def can_manage_employees(user_role):
    """Check if user role can manage employees"""
    return user_role == 'admin'

def can_manage_departments(user_role):
    """Check if user role can manage departments"""
    return user_role == 'admin'

def can_perform_checkout(user_role):
    """Check if user role can perform checkouts"""
    return user_role in ['admin', 'security_staff']

def can_view_audit_trail(user_role):
    """Check if user role can view audit trail"""
    return user_role == 'admin'

def can_extend_checkout(user_role):
    """Check if user role can extend checkout periods"""
    return user_role in ['admin', 'security_staff']

def can_mark_lost(user_role):
    """Check if user role can mark items as lost"""
    return user_role in ['admin', 'security_staff']
