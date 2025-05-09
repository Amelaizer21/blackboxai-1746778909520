from app import db
from datetime import datetime

class Key(db.Model):
    """Key model for managing physical keys in the system"""
    __tablename__ = 'keys'

    id = db.Column(db.Integer, primary_key=True)
    key_number = db.Column(db.String(20), unique=True, nullable=False)  # e.g., ROS-KEY-001
    name = db.Column(db.String(100), nullable=False)
    location = db.Column(db.String(100), nullable=False)
    description = db.Column(db.Text)
    status = db.Column(db.String(20), default='available')  # available, checked_out, lost, retired
    key_type = db.Column(db.String(50))  # master, sub-master, regular
    photo_url = db.Column(db.String(255))
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    last_maintenance = db.Column(db.DateTime)
    
    # Relationships
    transactions = db.relationship('Transaction', backref='key', lazy=True)

    def __init__(self, key_number, name, location, description=None, key_type='regular', photo_url=None):
        self.key_number = key_number
        self.name = name
        self.location = location
        self.description = description
        self.key_type = key_type
        self.photo_url = photo_url

    def to_dict(self):
        """Convert key object to dictionary"""
        return {
            'id': self.id,
            'key_number': self.key_number,
            'name': self.name,
            'location': self.location,
            'description': self.description,
            'status': self.status,
            'key_type': self.key_type,
            'photo_url': self.photo_url,
            'created_at': self.created_at.isoformat(),
            'updated_at': self.updated_at.isoformat(),
            'last_maintenance': self.last_maintenance.isoformat() if self.last_maintenance else None,
            'current_checkout': self.get_current_checkout(),
            'authorized_departments': [dept.name for dept in self.authorized_departments]
        }

    def get_current_checkout(self):
        """Get details of current checkout if key is checked out"""
        active_transaction = next((t for t in self.transactions if not t.check_in_time), None)
        if active_transaction:
            return {
                'employee': active_transaction.employee.full_name,
                'checkout_time': active_transaction.check_out_time.isoformat(),
                'expected_return': active_transaction.expected_return_time.isoformat() if active_transaction.expected_return_time else None
            }
        return None

    def check_out(self, employee_id):
        """Mark key as checked out"""
        if self.status != 'available':
            raise ValueError(f"Key {self.key_number} is not available for checkout")
        self.status = 'checked_out'
        db.session.commit()

    def check_in(self):
        """Mark key as available"""
        if self.status != 'checked_out':
            raise ValueError(f"Key {self.key_number} is not checked out")
        self.status = 'available'
        db.session.commit()

    def mark_lost(self):
        """Mark key as lost"""
        self.status = 'lost'
        db.session.commit()

    def retire(self):
        """Mark key as retired"""
        self.status = 'retired'
        db.session.commit()

    def record_maintenance(self):
        """Record maintenance performed on the key"""
        self.last_maintenance = datetime.utcnow()
        db.session.commit()

    def is_available(self):
        """Check if key is available for checkout"""
        return self.status == 'available'

    def __repr__(self):
        return f'<Key {self.key_number}: {self.status}>'
