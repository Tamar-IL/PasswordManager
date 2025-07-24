import JSEncrypt from 'jsencrypt';

function generateKeys() {
  const encrypt = new JSEncrypt({ default_key_size: '2048' });
  
  console.log('=== COPY THESE KEYS ===');
  console.log('Private Key:');
  console.log(encrypt.getPrivateKey());
  
  console.log('\nPublic Key:');
  console.log(encrypt.getPublicKey());
}

generateKeys();