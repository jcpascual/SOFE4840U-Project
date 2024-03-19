/* https://jan-ivar.github.io/samples/src/content/insertable-streams/video-analyzer/js/adapter-shim.js */
/* Copyright (c) 2023 jan-ivar */
/* Licensed under BSD 3-Clause */

'use strict';

if (!window.RTCRtpScriptTransform &&
    !('transform' in window.RTCRtpSender.prototype) &&
    !('transform' in window.RTCRtpReceiver.prototype)) {
    window.RTCRtpScriptTransform = class RTCRtpScriptTransform {
        constructor(worker, options, transfer) {
            this._worker = worker;
            this._options = options;
            this._transfer = transfer;
        }
    };
    const prop = {
        get() {
            return this._transform || null;
        },
        set(transform) {
            if (transform && !(transform instanceof window.RTCRtpScriptTransform)) {
                throw new TypeError('expected window.RTCRtpScriptTransform');
            }
            this._transform = transform || null;
            if (!transform) return;
            const {readable, writable} = this.createEncodedStreams();
            transform._worker.postMessage({
                rtctransform: {
                    readable, writable, options: transform._options
                }
            }, [readable, writable, ...(transform._transfer || [])]);
        }
    };
    Object.defineProperty(window.RTCRtpSender.prototype, 'transform', prop);
    Object.defineProperty(window.RTCRtpReceiver.prototype, 'transform', prop);
}
