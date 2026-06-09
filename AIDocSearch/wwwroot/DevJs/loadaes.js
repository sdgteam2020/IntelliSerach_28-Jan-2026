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