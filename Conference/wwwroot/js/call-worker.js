let callKey;

self.onmessage = ({data}) => {
    if ("rtctransform" in data) {
        /* needed by chrome shim */
        onrtctransform({transformer: data.rtctransform});
    } else if ("callkey" in data) {
        callKey = data.callkey;
    }
};

async function encryptFrame(chunk, controller) {
    const firstFour = new Uint8Array(chunk.data, 0, 4);
    const everythingElse = new Uint8Array(chunk.data, 4);
    
    const iv = crypto.getRandomValues(new Uint8Array(12));
    
    const ciphertext = await crypto.subtle.encrypt(
        {
            name: "AES-GCM",
            iv: iv
        },
        callKey,
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
    console.log("encrypt frame");
    const firstFour = new Uint8Array(chunk.data, 0, 4);
    const iv = new Uint8Array(chunk.data, 4, 12);
    const ciphertext = new Uint8Array(chunk.data, 16);
    
    const plaintext = await crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: iv
        },
        callKey,
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