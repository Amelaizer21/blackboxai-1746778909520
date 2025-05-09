from app import db
from datetime import datetime

class Department(db.Model):
    """Department model for organizing employees and managing access permissions"""
    __tablename__ = 'departments'

    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(100), unique=True, nullable=False)
    description = db.Column(db.Text)
    access_level = db.Column(db.Integer, default=1)  # Higher number = more access
    created_at = db.Column(db.DateTime, default=datetime.utcnow)
    updated_at = db.Column(db.DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    # Relationships
    employees = db.relationship('Employee', backref='department', lazy=True)
    
    # Many-to-Many relationship with Keys (department permissions)
    key_permissions = db.relationship('Key',
                                    secondary='department_key_permissions',
                                    backref=db.backref('authorized_departments', lazy=True))

    def to_dict(self):
        """Convert department object to dictionary"""
        return {
            'id': self.id,
            'name': self.name,
            'description': self.description,
            'access_level': self.access_level,
            'created_at': self.created_at.isoformat(),
            'updated_at': self.updated_at.isoformat(),
            'employee_count': len(self.employees),
            'key_permissions': [key.id for key in self.key_permissions]
        }

    def __repr__(self):
        return f'<Department {self.name}>'

# Association table for Department-Key permissions
department_key_permissions = db.Table('department_key_permissions',
    db.Column('department_id', db.Integer, db.ForeignKey('departments.id'), primary_key=True),
    db.Column('key_id', db.Integer, db.ForeignKey('keys.id'), primary_key=True),
    db.Column('granted_at', db.DateTime, default=datetime.utcnow),
    db.Column('granted_by', db.Integer, db.ForeignKey('users.id'))
)
