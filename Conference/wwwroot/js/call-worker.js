let descramble = true;
onmessage = ({data}) => {
    if ("descramble" in data) descramble = data.descramble;
    if ("rtctransform" in data) onrtctransform({transformer: data.rtctransform}); /* needed by chrome shim: */
};

onrtctransform = async ({transformer: {readable, writable, options}}) => {
    await readable.pipeThrough(new TransformStream({transform})).pipeTo(writable);

    function transform(chunk, controller) {
        console.log("transform");
        const bytes = new Uint8Array(chunk.data);
        const offset = 4; /* leave the first 4 bytes alone in VP8 */
        for (let i = offset; i < bytes.length; i++) {
            bytes[i] = ~bytes[i]; /* XOR the rest */
        }
        if (options.side == "receive" && !descramble) {
            for (let i = offset+10; i < offset+12; i++) {
                bytes[i] = ~bytes[i]; /* reverse a few XOR for spectacle */
            }
        }
        controller.enqueue(chunk);
    }
};