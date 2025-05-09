import boto3
import json
from datetime import datetime
import os
from botocore.exceptions import ClientError
import logging
from config import Config

logger = logging.getLogger(__name__)

class CloudSync:
    """Utility class for syncing data to cloud storage (AWS S3)"""
    
    def __init__(self):
        """Initialize AWS S3 client"""
        try:
            self.s3_client = boto3.client(
                's3',
                aws_access_key_id=Config.AWS_ACCESS_KEY,
                aws_secret_access_key=Config.AWS_SECRET_KEY
            )
            self.bucket_name = Config.AWS_BUCKET_NAME
            
        except Exception as e:
            logger.error(f"Failed to initialize S3 client: {str(e)}")
            raise

    def backup_database(self, data: dict, backup_type: str = 'full'):
        """
        Backup database data to S3
        
        Args:
            data (dict): The data to backup
            backup_type (str): Type of backup ('full' or 'incremental')
        """
        try:
            # Generate backup filename with timestamp
            timestamp = datetime.utcnow().strftime('%Y%m%d_%H%M%S')
            filename = f"backup_{backup_type}_{timestamp}.json"
            
            # Convert data to JSON
            json_data = json.dumps(data, default=str)
            
            # Upload to S3
            self.s3_client.put_object(
                Bucket=self.bucket_name,
                Key=f"backups/{filename}",
                Body=json_data,
                ContentType='application/json'
            )
            
            logger.info(f"Successfully created backup: {filename}")
            return filename
            
        except Exception as e:
            logger.error(f"Backup failed: {str(e)}")
            raise

    def restore_from_backup(self, filename: str = None):
        """
        Restore data from a backup file
        
        Args:
            filename (str): Specific backup file to restore from.
                          If None, uses the most recent backup.
        
        Returns:
            dict: The restored data
        """
        try:
            if not filename:
                # Get the most recent backup
                response = self.s3_client.list_objects_v2(
                    Bucket=self.bucket_name,
                    Prefix='backups/'
                )
                
                if 'Contents' not in response:
                    raise ValueError("No backups found")
                
                # Sort by last modified and get the most recent
                backups = sorted(
                    response['Contents'],
                    key=lambda x: x['LastModified'],
                    reverse=True
                )
                filename = backups[0]['Key']
            
            # Download the backup file
            response = self.s3_client.get_object(
                Bucket=self.bucket_name,
                Key=filename
            )
            
            # Parse JSON data
            backup_data = json.loads(response['Body'].read().decode('utf-8'))
            
            logger.info(f"Successfully restored from backup: {filename}")
            return backup_data
            
        except Exception as e:
            logger.error(f"Restore failed: {str(e)}")
            raise

    def list_backups(self):
        """
        List all available backups
        
        Returns:
            list: List of backup information
        """
        try:
            response = self.s3_client.list_objects_v2(
                Bucket=self.bucket_name,
                Prefix='backups/'
            )
            
            if 'Contents' not in response:
                return []
            
            backups = []
            for item in response['Contents']:
                backups.append({
                    'filename': item['Key'],
                    'size': item['Size'],
                    'last_modified': item['LastModified'].isoformat()
                })
            
            return backups
            
        except Exception as e:
            logger.error(f"Failed to list backups: {str(e)}")
            raise

    def delete_backup(self, filename: str):
        """
        Delete a specific backup
        
        Args:
            filename (str): Name of the backup file to delete
        """
        try:
            self.s3_client.delete_object(
                Bucket=self.bucket_name,
                Key=filename
            )
            
            logger.info(f"Successfully deleted backup: {filename}")
            
        except Exception as e:
            logger.error(f"Failed to delete backup: {str(e)}")
            raise

    def create_scheduled_backup(self):
        """Create a scheduled backup of the entire database"""
        try:
            # Get all data that needs to be backed up
            backup_data = {
                'metadata': {
                    'timestamp': datetime.utcnow().isoformat(),
                    'version': '1.0'
                },
                'users': self._get_users_data(),
                'employees': self._get_employees_data(),
                'departments': self._get_departments_data(),
                'keys': self._get_keys_data(),
                'access_cards': self._get_access_cards_data(),
                'transactions': self._get_transactions_data()
            }
            
            return self.backup_database(backup_data)
            
        except Exception as e:
            logger.error(f"Scheduled backup failed: {str(e)}")
            raise

    def _get_users_data(self):
        """Get all user data for backup"""
        from models.user import User
        users = User.query.all()
        return [user.to_dict() for user in users]

    def _get_employees_data(self):
        """Get all employee data for backup"""
        from models.employee import Employee
        employees = Employee.query.all()
        return [employee.to_dict() for employee in employees]

    def _get_departments_data(self):
        """Get all department data for backup"""
        from models.department import Department
        departments = Department.query.all()
        return [dept.to_dict() for dept in departments]

    def _get_keys_data(self):
        """Get all key data for backup"""
        from models.key import Key
        keys = Key.query.all()
        return [key.to_dict() for key in keys]

    def _get_access_cards_data(self):
        """Get all access card data for backup"""
        from models.access_card import AccessCard
        cards = AccessCard.query.all()
        return [card.to_dict() for card in cards]

    def _get_transactions_data(self):
        """Get all transaction data for backup"""
        from models.transaction import Transaction
        transactions = Transaction.query.all()
        return [transaction.to_dict() for transaction in transactions]

# Create a singleton instance
cloud_sync = CloudSync()

# Example usage:
"""
# Create a backup
backup_file = cloud_sync.create_scheduled_backup()

# List all backups
backups = cloud_sync.list_backups()

# Restore from most recent backup
restored_data = cloud_sync.restore_from_backup()

# Delete an old backup
cloud_sync.delete_backup('backups/old_backup.json')
"""
