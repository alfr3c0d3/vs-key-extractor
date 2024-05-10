import WinReg from 'winreg';
import dpapi from 'node-dpapi2';
import { readFileSync } from 'fs';

const products = JSON.parse(readFileSync('../products.json', 'utf8'));
const keyPattern = /\w{5}-\w{5}-\w{5}-\w{5}-\w{5}/;

products.forEach(product => {
    const regKey = new WinReg({
        hive: WinReg.HKCR, // HKEY_CLASSES_ROOT
        key: `\\Licenses\\${product.guid}\\${product.mpc}`
    });

    regKey.get('', (err, item) => {
        if (err) {
            return;
        }

        if (item?.value) {
            try {
                const decrypted = dpapi.unprotectData(Buffer.from(item.value, 'hex'), null, 'CurrentUser');
                const match = decrypted.toString('utf16le').match(keyPattern);

                if (match)
                    console.log(`Found key for ${product.name}: ${match[0]}`);

            } catch (decryptionError) {
                console.error(`Failed to decrypt key for ${product.name}: ${decryptionError}`);
            }
        } else {
            console.log(`No key found for ${product.name}`);
        }
    });
});
