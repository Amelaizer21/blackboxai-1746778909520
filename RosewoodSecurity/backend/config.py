import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    # Database
    SQLALCHEMY_DATABASE_URI = os.getenv('DATABASE_URL', 'postgresql://postgres:postgres@localhost/rosewood_security')
    SQLALCHEMY_TRACK_MODIFICATIONS = False
    
    # JWT Configuration
    JWT_SECRET_KEY = os.getenv('JWT_SECRET_KEY', 'your-secret-key-here')
    JWT_ACCESS_TOKEN_EXPIRES = 3600  # 1 hour
    
    # AES Encryption Key (32 bytes for AES-256)
    ENCRYPTION_KEY = os.getenv('ENCRYPTION_KEY', 'your-32-byte-encryption-key-here')
    
    # AWS S3 Configuration for backup
    AWS_ACCESS_KEY = os.getenv('AWS_ACCESS_KEY')
    AWS_SECRET_KEY = os.getenv('AWS_SECRET_KEY')
    AWS_BUCKET_NAME = os.getenv('AWS_BUCKET_NAME', 'rosewood-security-backup')
    
    # Flask Configuration
    DEBUG = os.getenv('FLASK_DEBUG', 'True') == 'True'
    SECRET_KEY = os.getenv('FLASK_SECRET_KEY', 'your-flask-secret-key-here')
    
    # 2FA Configuration
    ENABLE_2FA = os.getenv('ENABLE_2FA', 'True') == 'True'
    SMS_API_KEY = os.getenv('SMS_API_KEY')
    SMTP_SERVER = os.getenv('SMTP_SERVER', 'smtp.gmail.com')
    SMTP_PORT = int(os.getenv('SMTP_PORT', '587'))
    SMTP_USERNAME = os.getenv('SMTP_USERNAME')
    SMTP_PASSWORD = os.getenv('SMTP_PASSWORD')
