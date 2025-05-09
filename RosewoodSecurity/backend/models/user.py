from datetime import datetime
from app import db
from werkzeug.security import generate_password_hash, check_password_hash

class User(db.Model):
    """User model for authentication and role-based access control"""
    __tablename__ = 'users'

    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(80), unique=True, nullable=False)
    email = db.Column(db.String(120), unique=True, nullable=False)
    password_hash = db.Column(db.String(256), nullable=False)
    role = db.Column(db.String(20), nullable=False)  # 'admin', 'security_staff', 'auditor'
    is_active = db.Column(db.Boolean, default=True)
    two_fa_enabled = db.Column(db.Boolean, default=False)
    two_fa_secret = db.Column(db.String(32))
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    last_login = db.Column(db.DateTime)

    def __init__(self, username, email, password, role='security_staff'):
        self.username = username
        self.email = email
        self.set_password(password)
        self.role = role

    def set_password(self, password):
        """Hash and set the user's password"""
        self.password_hash = generate_password_hash(password)

    def check_password(self, password):
        """Verify the user's password"""
        return check_password_hash(self.password_hash, password)

    def to_dict(self):
        """Convert user object to dictionary (excluding sensitive data)"""
        return {
            'id': self.id,
            'username': self.username,
            'email': self.email,
            'role': self.role,
            'is_active': self.is_active,
            'two_fa_enabled': self.two_fa_enabled,
            'created_at': self.created_at.isoformat(),
            'last_login': self.last_login.isoformat() if self.last_login else None
        }

    def has_permission(self, required_role):
        """Check if user has required role permissions"""
        role_hierarchy = {
            'admin': ['admin', 'security_staff', 'auditor'],
            'security_staff': ['security_staff', 'auditor'],
            'auditor': ['auditor']
        }
        return required_role in role_hierarchy.get(self.role, [])

    def update_last_login(self):
        """Update the last login timestamp"""
        self.last_login = datetime.utcnow()
        db.session.commit()

    def __repr__(self):
        return f'<User {self.username}>'
