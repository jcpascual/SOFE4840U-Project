onmessage = ({data}) => {
    if ("rtctransform" in data) {
        /* needed by chrome shim */
        onrtctransform({transformer: data.rtctransform});
    }
};

async function getKey(roomId, password) {
    const encoder = new TextEncoder();
    
    const keyMaterial = await crypto.subtle.importKey(
        "raw",
        encoder.encode(password),
        "PBKDF2",
        false,
        ["deriveBits", "deriveKey"],
    );
    
    const salt = await crypto.subtle.digest("SHA-256", encoder.encode(roomId));
    
    return crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            hash: "SHA-256",
            salt: salt,
            iterations: 100000,
        },
        keyMaterial,
        {
            name: "AES-GCM", length: 256
        },
        true,
        ["encrypt", "decrypt"],
    );
}

async function encryptFrame(chunk, controller) {
    const firstFour = new Uint8Array(chunk.data, 0, 4);
    const everythingElse = new Uint8Array(chunk.data, 4);
    
    const key = await getKey("1234567890", "password");
    const iv = crypto.getRandomValues(new Uint8Array(12));
    
    const ciphertext = await crypto.subtle.encrypt(
        {
            name: "AES-GCM",
            iv: iv
        },
        key,
        everythingElse
    );
    
    const encryptedData = new ArrayBuffer(firstFour.byteLength + iv.byteLength + ciphertext.byteLength);
    const encryptedDataArray = new Uint8Array(encryptedData);
    
    encryptedDataArray.set(firstFour);
    encryptedDataArray.set(iv, 4);
    encryptedDataArray.set(new Uint8Array(ciphertext), 16);
    
    chunk.data = encryptedData;
    
    controller.enqueue(chunk);
}

async function decryptFrame(chunk, controller) {
    const firstFour = new Uint8Array(chunk.data, 0, 4);
    const iv = new Uint8Array(chunk.data, 4, 12);
    const ciphertext = new Uint8Array(chunk.data, 16);
    
    const key = await getKey("1234567890", "password");

    const plaintext = await crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: iv
        },
        key,
        ciphertext
    );

    const decryptedData = new ArrayBuffer(firstFour.byteLength + plaintext.byteLength);
    const decryptedDataArray = new Uint8Array(decryptedData);

    decryptedDataArray.set(firstFour);
    decryptedDataArray.set(new Uint8Array(plaintext), 4);

    chunk.data = decryptedData;

    controller.enqueue(chunk);
}

onrtctransform = async ({transformer: {readable, writable, options}}) => {
    let transformFunction;
    
    if (options.side === "receive") {
        transformFunction = decryptFrame;
    } else {
        transformFunction = encryptFrame;
    }
    
    await readable.pipeThrough(new TransformStream({
        transform: transformFunction
    })).pipeTo(writable);
};