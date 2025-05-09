from cryptography.fernet import Fernet
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.backends import default_backend
import base64
import os
from config import Config

class Encryption:
    """Utility class for handling encryption and decryption of sensitive data"""
    
    def __init__(self):
        """Initialize encryption with key from config"""
        self.fernet = self._setup_encryption()

    def _setup_encryption(self):
        """Set up the Fernet encryption using the configured key"""
        try:
            # Use the configured encryption key or generate a new one
            key = Config.ENCRYPTION_KEY.encode()
            if len(key) != 32:
                raise ValueError("Encryption key must be 32 bytes")
                
            # Generate a Fernet key from the encryption key
            kdf = PBKDF2HMAC(
                algorithm=hashes.SHA256(),
                length=32,
                salt=b'rosewood_security_salt',  # Fixed salt for consistent key derivation
                iterations=100000,
                backend=default_backend()
            )
            key = base64.urlsafe_b64encode(kdf.derive(key))
            return Fernet(key)
            
        except Exception as e:
            raise RuntimeError(f"Failed to setup encryption: {str(e)}")

    def encrypt(self, data: str) -> str:
        """
        Encrypt a string of data
        
        Args:
            data (str): The data to encrypt
            
        Returns:
            str: The encrypted data as a base64-encoded string
        """
        try:
            if not isinstance(data, str):
                raise ValueError("Data must be a string")
                
            # Convert string to bytes and encrypt
            encrypted_data = self.fernet.encrypt(data.encode())
            
            # Convert to base64 string for storage
            return base64.urlsafe_b64encode(encrypted_data).decode()
            
        except Exception as e:
            raise RuntimeError(f"Encryption failed: {str(e)}")

    def decrypt(self, encrypted_data: str) -> str:
        """
        Decrypt an encrypted string
        
        Args:
            encrypted_data (str): The encrypted data as a base64-encoded string
            
        Returns:
            str: The decrypted data
        """
        try:
            if not isinstance(encrypted_data, str):
                raise ValueError("Encrypted data must be a string")
                
            # Convert from base64 string and decrypt
            encrypted_bytes = base64.urlsafe_b64decode(encrypted_data.encode())
            decrypted_data = self.fernet.decrypt(encrypted_bytes)
            
            # Convert bytes back to string
            return decrypted_data.decode()
            
        except Exception as e:
            raise RuntimeError(f"Decryption failed: {str(e)}")

    def encrypt_dict(self, data: dict, sensitive_fields: list) -> dict:
        """
        Encrypt sensitive fields in a dictionary
        
        Args:
            data (dict): The dictionary containing data to partially encrypt
            sensitive_fields (list): List of field names to encrypt
            
        Returns:
            dict: Dictionary with specified fields encrypted
        """
        try:
            encrypted_data = data.copy()
            for field in sensitive_fields:
                if field in encrypted_data and encrypted_data[field]:
                    encrypted_data[field] = self.encrypt(str(encrypted_data[field]))
            return encrypted_data
            
        except Exception as e:
            raise RuntimeError(f"Dictionary encryption failed: {str(e)}")

    def decrypt_dict(self, data: dict, sensitive_fields: list) -> dict:
        """
        Decrypt sensitive fields in a dictionary
        
        Args:
            data (dict): The dictionary containing encrypted data
            sensitive_fields (list): List of field names to decrypt
            
        Returns:
            dict: Dictionary with specified fields decrypted
        """
        try:
            decrypted_data = data.copy()
            for field in sensitive_fields:
                if field in decrypted_data and decrypted_data[field]:
                    decrypted_data[field] = self.decrypt(str(decrypted_data[field]))
            return decrypted_data
            
        except Exception as e:
            raise RuntimeError(f"Dictionary decryption failed: {str(e)}")

    @staticmethod
    def generate_key() -> str:
        """
        Generate a new random encryption key
        
        Returns:
            str: A new 32-byte key encoded as base64
        """
        try:
            key = os.urandom(32)
            return base64.urlsafe_b64encode(key).decode()
            
        except Exception as e:
            raise RuntimeError(f"Key generation failed: {str(e)}")

# Create a singleton instance
encryption = Encryption()

# Example usage:
"""
# Encrypting sensitive data
encrypted_data = encryption.encrypt("sensitive information")

# Decrypting data
decrypted_data = encryption.decrypt(encrypted_data)

# Encrypting specific fields in a dictionary
data = {
    'id': 123,
    'name': 'John Doe',
    'ssn': '123-45-6789'  # sensitive field
}
encrypted = encryption.encrypt_dict(data, sensitive_fields=['ssn'])

# Decrypting specific fields in a dictionary
decrypted = encryption.decrypt_dict(encrypted, sensitive_fields=['ssn'])
"""
