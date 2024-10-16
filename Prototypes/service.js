// Optional JS file for form validation (service.js)
document.querySelector("form").addEventListener("submit", function(event) {
    let checkInTime = document.getElementById("CheckInTime").value;
    let checkOutTime = document.getElementById("CheckOutTime").value;

    if (new Date(checkInTime) > new Date(checkOutTime)) {
        alert("Check-out time must be after check-in time!");
        event.preventDefault();
    }
});

// Show the relevant verification section based on the selected method
document.querySelectorAll('input[name="VerificationMethod"]').forEach((input) => {
    input.addEventListener('change', function () {
        const qrSection = document.getElementById('qrCodeSection');
        const pinSection = document.getElementById('pinSection');
        
        if (this.value === 'QR Code') {
            qrSection.classList.remove('hidden');
            pinSection.classList.add('hidden');
        } else {
            qrSection.classList.add('hidden');
            pinSection.classList.remove('hidden');
        }
    });
});

// Start/End Service
document.addEventListener("DOMContentLoaded", function () {
    const qrCodeRadio = document.getElementById("qrcode");
    const pinRadio = document.getElementById("pin");
    const qrCodeSection = document.getElementById("qrCodeSection");
    const pinSection = document.getElementById("pinSection");
    const startScanButton = document.getElementById("startScan");
    const stopScanButton = document.getElementById("stopScan");
    const qrCodeResultInput = document.getElementById("qrCodeResult");

    let html5QrCode;

    // Initialize QR Code scanner
    function initQrCodeScanner() {
        html5QrCode = new Html5Qrcode("qrCodeReader");

        startScanButton.onclick = () => {
            html5QrCode.start(
                { facingMode: "environment" }, // Use the rear camera
                {
                    fps: 10, // Set the frame rate
                    qrbox: { width: 250, height: 250 } // Set the size of the scanning box
                },
                (decodedText, decodedResult) => {
                    // Handle successful QR code scan
                    qrCodeResultInput.value = decodedText; // Store the scanned QR code result
                    stopQrCodeScanning();
                    alert(`QR Code scanned: ${decodedText}`);
                },
                (errorMessage) => {
                    // Handle error during scanning
                    console.error(`QR Code scan error: ${errorMessage}`);
                }
            ).catch(err => {
                console.error(`Failed to start QR Code scanning: ${err}`);
            });

            startScanButton.classList.add("hidden");
            stopScanButton.classList.remove("hidden");
        };

        stopScanButton.onclick = stopQrCodeScanning;
    }

    function stopQrCodeScanning() {
        if (html5QrCode) {
            html5QrCode.stop().then(ignore => {
                startScanButton.classList.remove("hidden");
                stopScanButton.classList.add("hidden");
            }).catch(err => {
                console.error(`Failed to stop QR Code scanning: ${err}`);
            });
        }
    }

    // Initial visibility setup
    updateVerificationSection();
    initQrCodeScanner();
});
