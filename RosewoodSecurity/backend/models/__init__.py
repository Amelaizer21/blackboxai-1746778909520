from flask_sqlalchemy import SQLAlchemy
from app import db

# Import all models here to make them available when importing models
from .user import User
from .employee import Employee
from .department import Department
from .key import Key
from .access_card import AccessCard
from .transaction import Transaction

# Initialize models
def init_models():
    db.create_all()
