mMsater(0, "RankId", 1, "");

document.querySelector("form").addEventListener("submit", function (e) {
    e.preventDefault();

    const passwordInput = document.querySelector("#Password");
    const plainPassword = passwordInput.value;

    const secretKey = document.getElementById("aesKey").value;
    const encryptedPassword = encryptData(plainPassword, secretKey);
    passwordInput.value = encryptedPassword;

    const CpasswordInput = document.querySelector("#ConfirmPassword");
    const CplainPassword = CpasswordInput.value;

    const CencryptedPassword = encryptData(CplainPassword, secretKey);

    CpasswordInput.value = CencryptedPassword;

    this.submit();
});

function encryptData(plainText, secret) {
    const hash = CryptoJS.SHA256(secret).toString();

    const key = CryptoJS.enc.Hex.parse(hash.substring(0, 64)); // 32 bytes
    const iv = CryptoJS.enc.Hex.parse(hash.substring(0, 32)); // 16 bytes

    return CryptoJS.AES.encrypt(plainText, key, {
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    }).toString();
}

