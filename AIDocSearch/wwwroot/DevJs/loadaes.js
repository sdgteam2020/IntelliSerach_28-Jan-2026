function encryptPayloadData(plainText) {
    const secret = document.getElementById("payloadaes").value;

    const hash = CryptoJS.SHA256(secret).toString();

    const key = CryptoJS.enc.Hex.parse(hash.substring(0, 64)); // 32 bytes
    const iv = CryptoJS.enc.Hex.parse(hash.substring(0, 32)); // 16 bytes

    const encrypted = CryptoJS.AES.encrypt(plainText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    return encrypted.toString();
} 
function decryptPayloadData(cipherText) {
    const secret = document.getElementById("payloadaes").value;

    const hash = CryptoJS.SHA256(secret).toString();

    const key = CryptoJS.enc.Hex.parse(hash.substring(0, 64)); // 32 bytes
    const iv = CryptoJS.enc.Hex.parse(hash.substring(0, 32));  // 16 bytes

    const decrypted = CryptoJS.AES.decrypt(cipherText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    return decrypted.toString(CryptoJS.enc.Utf8);
}


function encryptPayloadDatawithkey(plainText, secret) {


    const hash = CryptoJS.SHA256(secret).toString();

    const key = CryptoJS.enc.Hex.parse(hash.substring(0, 64)); // 32 bytes
    const iv = CryptoJS.enc.Hex.parse(hash.substring(0, 32)); // 16 bytes

    const encrypted = CryptoJS.AES.encrypt(plainText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    return encrypted.toString();
}
function decryptPayloadDatawithkey(cipherText, secret) {


    const hash = CryptoJS.SHA256(secret).toString();

    const key = CryptoJS.enc.Hex.parse(hash.substring(0, 64)); // 32 bytes
    const iv = CryptoJS.enc.Hex.parse(hash.substring(0, 32));  // 16 bytes

    const decrypted = CryptoJS.AES.decrypt(cipherText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });

    return decrypted.toString(CryptoJS.enc.Utf8);
}

const CACHE_NAME = "UserActivityCache";

// ===============================
// SAVE DATA IN CACHE
// ===============================
async function saveUserActivity(encrypted, STORAGE_KEY) {

    const cache = await caches.open(CACHE_NAME);

    const response = new Response(encrypted, {
        headers: {
            "Content-Type": "text/plain"
        }
    });

    await cache.put(
        new Request("/cache/" + STORAGE_KEY),
        response
    );
}
async function getUserActivity(STORAGE_KEY) {

    const cache = await caches.open(CACHE_NAME);

    const response = await cache.match(
        "/cache/" + STORAGE_KEY
    );

    if (!response)
        return null;

    const response1=await response.text()
    let data = decryptPayloadDatawithkey(response1, $('#spnUserName').val());
    return data;
}