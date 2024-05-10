import json
import re
import winreg
import win32crypt
from winreg import HKEY_CLASSES_ROOT

# Load product data from a JSON file located one directory level up
with open('../products.json', 'r') as file:
    products = json.load(file)

def extract_license(product):
    # Construct the registry path using product details
    path = f'Licenses\\{product["guid"]}\\{product["mpc"]}'
    try:
        # Open the registry key
        with winreg.OpenKey(HKEY_CLASSES_ROOT, path) as key:
            # Get the encrypted data
            encrypted_data, _ = winreg.QueryValueEx(key, "")
    except FileNotFoundError:
        # Handle missing registry key silently
        return
    except Exception as e:
        print(f"Failed to read registry for {product['name']}: {e}")
        return

    try:
        # Decrypt the data using DPAPI
        decrypted_data = win32crypt.CryptUnprotectData(encrypted_data, None, None, None, 0)[1]
    except Exception as e:
        print(f"Decryption failed for {product['name']}: {e}")
        return

    # Decode the data to a string (assuming it's UTF-16 encoded)
    decoded_str = decrypted_data.decode('utf-16le')
    # Search for the license key pattern
    for sub in decoded_str.split('\0'):
        match = re.search(r'\w{5}-\w{5}-\w{5}-\w{5}-\w{5}', sub)
        if match:
            print(f"Found key for {product['name']}: {match.group(0)}")

for product in products:
    extract_license(product)
