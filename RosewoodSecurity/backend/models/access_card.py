from app import db
from datetime import datetime

class AccessCard(db.Model):
    """AccessCard model for managing electronic access cards"""
    __tablename__ = 'access_cards'

    id = db.Column(db.Integer, primary_key=True)
    card_number = db.Column(db.String(20), unique=True, nullable=False)  # e.g., ROS-CARD-001
    card_type = db.Column(db.String(50), nullable=False)  # permanent, temporary, visitor
    status = db.Column(db.String(20), default='active')  # active, inactive, lost, expired
    issue_date = db.Column(db.DateTime, default=datetime.utcnow)
    expiry_date = db.Column(db.DateTime)
    access_zones = db.Column(db.JSON)  # List of authorized zones/areas
    employee_id = db.Column(db.Integer, db.ForeignKey('employees.id'))
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    last_used = db.Column(db.DateTime)
    
    # Relationships
    transactions = db.relationship('Transaction', backref='access_card', lazy=True)

    def __init__(self, card_number, card_type, access_zones, employee_id=None, expiry_date=None):
        self.card_number = card_number
        self.card_type = card_type
        self.access_zones = access_zones
        self.employee_id = employee_id
        self.expiry_date = expiry_date

    def to_dict(self):
        """Convert access card object to dictionary"""
        return {
            'id': self.id,
            'card_number': self.card_number,
            'card_type': self.card_type,
            'status': self.status,
            'issue_date': self.issue_date.isoformat(),
            'expiry_date': self.expiry_date.isoformat() if self.expiry_date else None,
            'access_zones': self.access_zones,
            'employee_id': self.employee_id,
            'employee_name': self.assigned_employee.full_name if self.assigned_employee else None,
            'created_at': self.created_at.isoformat(),
            'updated_at': self.updated_at.isoformat(),
            'last_used': self.last_used.isoformat() if self.last_used else None,
            'is_expired': self.is_expired()
        }

    def is_expired(self):
        """Check if the access card has expired"""
        if not self.expiry_date:
            return False
        return datetime.utcnow() > self.expiry_date

    def deactivate(self):
        """Deactivate the access card"""
        self.status = 'inactive'
        db.session.commit()

    def activate(self):
        """Activate the access card"""
        if self.is_expired():
            raise ValueError(f"Cannot activate expired card {self.card_number}")
        self.status = 'active'
        db.session.commit()

    def mark_lost(self):
        """Mark the access card as lost"""
        self.status = 'lost'
        db.session.commit()

    def record_usage(self):
        """Record when the card was last used"""
        self.last_used = datetime.utcnow()
        db.session.commit()

    def assign_to_employee(self, employee_id):
        """Assign the card to an employee"""
        self.employee_id = employee_id
        db.session.commit()

    def unassign(self):
        """Unassign the card from current employee"""
        self.employee_id = None
        db.session.commit()

    def update_access_zones(self, zones):
        """Update the card's access zones"""
        self.access_zones = zones
        self.updated_at = datetime.utcnow()
        db.session.commit()

    def extend_expiry(self, new_expiry_date):
        """Extend the card's expiry date"""
        if new_expiry_date <= datetime.utcnow():
            raise ValueError("New expiry date must be in the future")
        self.expiry_date = new_expiry_date
        db.session.commit()

    def __repr__(self):
        return f'<AccessCard {self.card_number}: {self.status}>'
