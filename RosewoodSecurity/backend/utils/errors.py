from flask import jsonify
from werkzeug.exceptions import HTTPException
import traceback
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class APIError(Exception):
    """Base exception class for API errors"""
    def __init__(self, message, status_code=400, payload=None):
        super().__init__()
        self.message = message
        self.status_code = status_code
        self.payload = payload

    def to_dict(self):
        rv = dict(self.payload or ())
        rv['error'] = self.message
        rv['status_code'] = self.status_code
        return rv

class ValidationError(APIError):
    """Exception raised for validation errors"""
    def __init__(self, message, payload=None):
        super().__init__(message, status_code=400, payload=payload)

class AuthenticationError(APIError):
    """Exception raised for authentication errors"""
    def __init__(self, message, payload=None):
        super().__init__(message, status_code=401, payload=payload)

class AuthorizationError(APIError):
    """Exception raised for authorization errors"""
    def __init__(self, message, payload=None):
        super().__init__(message, status_code=403, payload=payload)

class ResourceNotFoundError(APIError):
    """Exception raised when a resource is not found"""
    def __init__(self, message, payload=None):
        super().__init__(message, status_code=404, payload=payload)

class ConflictError(APIError):
    """Exception raised for conflicts with existing resources"""
    def __init__(self, message, payload=None):
        super().__init__(message, status_code=409, payload=payload)

def register_error_handlers(app):
    """Register error handlers for the Flask app"""
    
    @app.errorhandler(APIError)
    def handle_api_error(error):
        """Handle custom API errors"""
        response = jsonify(error.to_dict())
        response.status_code = error.status_code
        return response

    @app.errorhandler(400)
    def handle_bad_request(error):
        """Handle bad request errors"""
        return jsonify({
            'error': 'Bad request',
            'message': str(error),
            'status_code': 400
        }), 400

    @app.errorhandler(401)
    def handle_unauthorized(error):
        """Handle unauthorized access errors"""
        return jsonify({
            'error': 'Unauthorized',
            'message': 'Authentication required',
            'status_code': 401
        }), 401

    @app.errorhandler(403)
    def handle_forbidden(error):
        """Handle forbidden access errors"""
        return jsonify({
            'error': 'Forbidden',
            'message': 'You do not have permission to perform this action',
            'status_code': 403
        }), 403

    @app.errorhandler(404)
    def handle_not_found(error):
        """Handle resource not found errors"""
        return jsonify({
            'error': 'Not found',
            'message': 'The requested resource was not found',
            'status_code': 404
        }), 404

    @app.errorhandler(405)
    def handle_method_not_allowed(error):
        """Handle method not allowed errors"""
        return jsonify({
            'error': 'Method not allowed',
            'message': 'The method is not allowed for this endpoint',
            'status_code': 405
        }), 405

    @app.errorhandler(409)
    def handle_conflict(error):
        """Handle resource conflict errors"""
        return jsonify({
            'error': 'Conflict',
            'message': str(error),
            'status_code': 409
        }), 409

    @app.errorhandler(422)
    def handle_unprocessable_entity(error):
        """Handle unprocessable entity errors"""
        return jsonify({
            'error': 'Unprocessable entity',
            'message': str(error),
            'status_code': 422
        }), 422

    @app.errorhandler(429)
    def handle_rate_limit_exceeded(error):
        """Handle rate limit exceeded errors"""
        return jsonify({
            'error': 'Too many requests',
            'message': 'Rate limit exceeded. Please try again later.',
            'status_code': 429
        }), 429

    @app.errorhandler(500)
    def handle_internal_server_error(error):
        """Handle internal server errors"""
        # Log the error for debugging
        logger.error(f"Internal Server Error: {str(error)}")
        logger.error(traceback.format_exc())
        
        return jsonify({
            'error': 'Internal server error',
            'message': 'An unexpected error occurred',
            'status_code': 500
        }), 500

    @app.errorhandler(HTTPException)
    def handle_http_exception(error):
        """Handle all other HTTP exceptions"""
        return jsonify({
            'error': error.name,
            'message': error.description,
            'status_code': error.code
        }), error.code

    @app.errorhandler(Exception)
    def handle_generic_exception(error):
        """Handle all unhandled exceptions"""
        # Log the error for debugging
        logger.error(f"Unhandled Exception: {str(error)}")
        logger.error(traceback.format_exc())
        
        return jsonify({
            'error': 'Internal server error',
            'message': 'An unexpected error occurred',
            'status_code': 500
        }), 500

def validate_request_data(data, required_fields):
    """Validate that all required fields are present in the request data"""
    missing_fields = [field for field in required_fields if field not in data]
    if missing_fields:
        raise ValidationError(
            f"Missing required fields: {', '.join(missing_fields)}"
        )

def validate_field_type(value, field_name, expected_type):
    """Validate that a field value is of the expected type"""
    if not isinstance(value, expected_type):
        raise ValidationError(
            f"Field '{field_name}' must be of type {expected_type.__name__}"
        )

def validate_field_length(value, field_name, min_length=None, max_length=None):
    """Validate that a field value meets length requirements"""
    if min_length is not None and len(value) < min_length:
        raise ValidationError(
            f"Field '{field_name}' must be at least {min_length} characters long"
        )
    if max_length is not None and len(value) > max_length:
        raise ValidationError(
            f"Field '{field_name}' must not exceed {max_length} characters"
        )

def validate_enum_value(value, field_name, valid_values):
    """Validate that a field value is one of the allowed values"""
    if value not in valid_values:
        raise ValidationError(
            f"Invalid value for '{field_name}'. Must be one of: {', '.join(valid_values)}"
        )
