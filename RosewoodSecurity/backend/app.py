from flask import Flask, jsonify
from flask_sqlalchemy import SQLAlchemy
from flask_jwt_extended import JWTManager
from config import Config

# Initialize Flask app
app = Flask(__name__)
app.config.from_object(Config)

# Initialize extensions
db = SQLAlchemy(app)
jwt = JWTManager(app)

# Import routes after db initialization to avoid circular imports
from routes.auth import auth_bp
from routes.dashboard import dashboard_bp
from routes.keys import keys_bp
from routes.access_cards import access_cards_bp
from routes.employees import employees_bp
from routes.transactions import transactions_bp
from routes.reports import reports_bp

# Register blueprints
app.register_blueprint(auth_bp, url_prefix='/auth')
app.register_blueprint(dashboard_bp, url_prefix='/dashboard')
app.register_blueprint(keys_bp, url_prefix='/keys')
app.register_blueprint(access_cards_bp, url_prefix='/access-cards')
app.register_blueprint(employees_bp, url_prefix='/employees')
app.register_blueprint(transactions_bp, url_prefix='/transactions')
app.register_blueprint(reports_bp, url_prefix='/reports')

# Error handlers
@app.errorhandler(404)
def not_found(error):
    return jsonify({'error': 'Resource not found'}), 404

@app.errorhandler(500)
def internal_error(error):
    return jsonify({'error': 'Internal server error'}), 500

@app.route('/health')
def health_check():
    """Health check endpoint"""
    return jsonify({'status': 'healthy', 'service': 'Rosewood Security API'})

if __name__ == '__main__':
    # Create all database tables
    with app.app_context():
        db.create_all()
    
    # Run the application with SSL in production
    app.run(host='0.0.0.0', port=5000, ssl_context='adhoc' if not app.debug else None)
