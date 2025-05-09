from flask import Blueprint, request, jsonify
from flask_jwt_extended import (
    create_access_token, 
    create_refresh_token,
    jwt_required,
    get_jwt_identity
)
from werkzeug.security import check_password_hash
from datetime import datetime, timedelta
import pyotp

from app import db
from models.user import User

auth_bp = Blueprint('auth', __name__)

@auth_bp.route('/login', methods=['POST'])
def login():
    """Handle user login with optional 2FA"""
    data = request.get_json()
    
    if not data or not data.get('username') or not data.get('password'):
        return jsonify({'error': 'Missing username or password'}), 400
    
    user = User.query.filter_by(username=data['username']).first()
    
    if not user or not user.check_password(data['password']):
        return jsonify({'error': 'Invalid username or password'}), 401
    
    if not user.is_active:
        return jsonify({'error': 'Account is inactive'}), 401
    
    # If 2FA is enabled for user, require verification code
    if user.two_fa_enabled:
        if not data.get('two_fa_code'):
            return jsonify({
                'message': '2FA code required',
                'requires_2fa': True
            }), 200
            
        totp = pyotp.TOTP(user.two_fa_secret)
        if not totp.verify(data['two_fa_code']):
            return jsonify({'error': 'Invalid 2FA code'}), 401
    
    # Update last login time
    user.update_last_login()
    
    # Generate tokens
    access_token = create_access_token(
        identity=user.id,
        additional_claims={'role': user.role}
    )
    refresh_token = create_refresh_token(identity=user.id)
    
    return jsonify({
        'access_token': access_token,
        'refresh_token': refresh_token,
        'user': user.to_dict()
    }), 200

@auth_bp.route('/refresh', methods=['POST'])
@jwt_required(refresh=True)
def refresh():
    """Refresh access token"""
    current_user_id = get_jwt_identity()
    user = User.query.get(current_user_id)
    
    if not user or not user.is_active:
        return jsonify({'error': 'Invalid or inactive user'}), 401
    
    access_token = create_access_token(
        identity=current_user_id,
        additional_claims={'role': user.role}
    )
    
    return jsonify({'access_token': access_token}), 200

@auth_bp.route('/setup-2fa', methods=['POST'])
@jwt_required()
def setup_2fa():
    """Set up 2FA for user"""
    current_user_id = get_jwt_identity()
    user = User.query.get(current_user_id)
    
    if not user:
        return jsonify({'error': 'User not found'}), 404
    
    if user.two_fa_enabled:
        return jsonify({'error': '2FA is already enabled'}), 400
    
    # Generate new secret key
    secret = pyotp.random_base32()
    totp = pyotp.TOTP(secret)
    provisioning_uri = totp.provisioning_uri(
        user.email,
        issuer_name="Rosewood Security"
    )
    
    # Save secret to user
    user.two_fa_secret = secret
    user.two_fa_enabled = True
    db.session.commit()
    
    return jsonify({
        'message': '2FA setup successful',
        'secret': secret,
        'qr_uri': provisioning_uri
    }), 200

@auth_bp.route('/disable-2fa', methods=['POST'])
@jwt_required()
def disable_2fa():
    """Disable 2FA for user"""
    current_user_id = get_jwt_identity()
    user = User.query.get(current_user_id)
    
    if not user:
        return jsonify({'error': 'User not found'}), 404
    
    if not user.two_fa_enabled:
        return jsonify({'error': '2FA is not enabled'}), 400
    
    # Disable 2FA
    user.two_fa_enabled = False
    user.two_fa_secret = None
    db.session.commit()
    
    return jsonify({'message': '2FA disabled successfully'}), 200

@auth_bp.route('/change-password', methods=['POST'])
@jwt_required()
def change_password():
    """Change user password"""
    data = request.get_json()
    current_user_id = get_jwt_identity()
    user = User.query.get(current_user_id)
    
    if not user:
        return jsonify({'error': 'User not found'}), 404
    
    if not data.get('current_password') or not data.get('new_password'):
        return jsonify({'error': 'Missing current or new password'}), 400
    
    if not user.check_password(data['current_password']):
        return jsonify({'error': 'Current password is incorrect'}), 401
    
    # Update password
    user.set_password(data['new_password'])
    db.session.commit()
    
    return jsonify({'message': 'Password changed successfully'}), 200

@auth_bp.route('/logout', methods=['POST'])
@jwt_required()
def logout():
    """Handle user logout"""
    # In a more complex implementation, you might want to blacklist the token
    return jsonify({'message': 'Logged out successfully'}), 200
