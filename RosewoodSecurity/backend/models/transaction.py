from app import db
from datetime import datetime

class Transaction(db.Model):
    """Transaction model for tracking key and access card checkouts"""
    __tablename__ = 'transactions'

    id = db.Column(db.Integer, primary_key=True)
    transaction_number = db.Column(db.String(30), unique=True, nullable=False)  # e.g., TRX-20240115-001
    employee_id = db.Column(db.Integer, db.ForeignKey('employees.id'), nullable=False)
    key_id = db.Column(db.Integer, db.ForeignKey('keys.id'))
    access_card_id = db.Column(db.Integer, db.ForeignKey('access_cards.id'))
    check_out_time = db.Column(db.DateTime, nullable=False, default=datetime.utcnow)
    expected_return_time = db.Column(db.DateTime)
    check_in_time = db.Column(db.DateTime)
    purpose = db.Column(db.String(200))
    status = db.Column(db.String(20), default='active')  # active, completed, overdue, lost
    notes = db.Column(db.Text)
    created_by = db.Column(db.Integer, db.ForeignKey('users.id'), nullable=False)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    def __init__(self, employee_id, created_by, key_id=None, access_card_id=None, 
                 expected_return_time=None, purpose=None):
        if not (key_id or access_card_id):
            raise ValueError("Either key_id or access_card_id must be provided")
        if key_id and access_card_id:
            raise ValueError("Transaction can't be for both key and access card")
            
        self.transaction_number = self._generate_transaction_number()
        self.employee_id = employee_id
        self.key_id = key_id
        self.access_card_id = access_card_id
        self.expected_return_time = expected_return_time
        self.purpose = purpose
        self.created_by = created_by

    def _generate_transaction_number(self):
        """Generate a unique transaction number"""
        date_str = datetime.utcnow().strftime('%Y%m%d')
        # Get count of transactions for today and add 1
        today_count = Transaction.query.filter(
            Transaction.transaction_number.like(f'TRX-{date_str}-%')
        ).count() + 1
        return f'TRX-{date_str}-{today_count:03d}'

    def to_dict(self):
        """Convert transaction object to dictionary"""
        return {
            'id': self.id,
            'transaction_number': self.transaction_number,
            'employee': {
                'id': self.employee_id,
                'name': self.employee.full_name
            },
            'item': {
                'type': 'key' if self.key_id else 'access_card',
                'id': self.key_id or self.access_card_id,
                'identifier': self.key.key_number if self.key_id else self.access_card.card_number
            },
            'check_out_time': self.check_out_time.isoformat(),
            'expected_return_time': self.expected_return_time.isoformat() if self.expected_return_time else None,
            'check_in_time': self.check_in_time.isoformat() if self.check_in_time else None,
            'purpose': self.purpose,
            'status': self.status,
            'notes': self.notes,
            'duration': self.get_duration(),
            'is_overdue': self.is_overdue()
        }

    def check_in(self, notes=None):
        """Process check-in of key/card"""
        if self.check_in_time:
            raise ValueError(f"Transaction {self.transaction_number} is already checked in")
            
        self.check_in_time = datetime.utcnow()
        self.status = 'completed'
        if notes:
            self.notes = notes

        # Update key/card status
        if self.key_id:
            self.key.check_in()
        else:
            self.access_card.record_usage()

        db.session.commit()

    def mark_lost(self, notes=None):
        """Mark the transaction as lost"""
        self.status = 'lost'
        if notes:
            self.notes = notes

        # Update key/card status
        if self.key_id:
            self.key.mark_lost()
        else:
            self.access_card.mark_lost()

        db.session.commit()

    def is_overdue(self):
        """Check if the transaction is overdue"""
        if not self.expected_return_time or self.check_in_time:
            return False
        return datetime.utcnow() > self.expected_return_time

    def get_duration(self):
        """Calculate the duration of the checkout in hours"""
        end_time = self.check_in_time or datetime.utcnow()
        duration = end_time - self.check_out_time
        return round(duration.total_seconds() / 3600, 2)  # Convert to hours

    def update_expected_return(self, new_time):
        """Update the expected return time"""
        if self.check_in_time:
            raise ValueError("Cannot update return time for completed transaction")
        self.expected_return_time = new_time
        db.session.commit()

    def add_notes(self, notes):
        """Add notes to the transaction"""
        self.notes = notes
        db.session.commit()

    def __repr__(self):
        return f'<Transaction {self.transaction_number}: {self.status}>'
