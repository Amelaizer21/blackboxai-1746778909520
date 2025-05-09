from app import db
from datetime import datetime

class Employee(db.Model):
    """Employee model for managing staff members who can check out keys/cards"""
    __tablename__ = 'employees'

    id = db.Column(db.Integer, primary_key=True)
    employee_number = db.Column(db.String(20), unique=True, nullable=False)
    first_name = db.Column(db.String(50), nullable=False)
    last_name = db.Column(db.String(50), nullable=False)
    email = db.Column(db.String(120), unique=True, nullable=False)
    phone = db.Column(db.String(20))
    department_id = db.Column(db.Integer, db.ForeignKey('departments.id'), nullable=False)
    photo_url = db.Column(db.String(255))
    status = db.Column(db.String(20), default='active')  # active, inactive, suspended
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    transactions = db.relationship('Transaction', backref='employee', lazy=True)
    access_cards = db.relationship('AccessCard', backref='assigned_employee', lazy=True)

    def __init__(self, employee_number, first_name, last_name, email, department_id, phone=None, photo_url=None):
        self.employee_number = employee_number
        self.first_name = first_name
        self.last_name = last_name
        self.email = email
        self.department_id = department_id
        self.phone = phone
        self.photo_url = photo_url

    @property
    def full_name(self):
        """Return the employee's full name"""
        return f"{self.first_name} {self.last_name}"

    def to_dict(self):
        """Convert employee object to dictionary"""
        return {
            'id': self.id,
            'employee_number': self.employee_number,
            'first_name': self.first_name,
            'last_name': self.last_name,
            'full_name': self.full_name,
            'email': self.email,
            'phone': self.phone,
            'department_id': self.department_id,
            'department_name': self.department.name if self.department else None,
            'photo_url': self.photo_url,
            'status': self.status,
            'created_at': self.created_at.isoformat(),
            'updated_at': self.updated_at.isoformat(),
            'active_checkouts': self.get_active_checkouts()
        }

    def get_active_checkouts(self):
        """Get list of currently checked out keys and cards"""
        active_transactions = [t for t in self.transactions if not t.check_in_time]
        return [{
            'type': 'key' if t.key_id else 'access_card',
            'item_id': t.key_id or t.access_card_id,
            'checkout_time': t.check_out_time.isoformat(),
            'expected_return': t.expected_return_time.isoformat() if t.expected_return_time else None
        } for t in active_transactions]

    def can_checkout_key(self, key_id):
        """Check if employee's department has permission for this key"""
        return key_id in [key.id for key in self.department.key_permissions]

    def suspend(self):
        """Suspend the employee"""
        self.status = 'suspended'
        db.session.commit()

    def activate(self):
        """Activate the employee"""
        self.status = 'active'
        db.session.commit()

    def __repr__(self):
        return f'<Employee {self.employee_number}: {self.full_name}>'
