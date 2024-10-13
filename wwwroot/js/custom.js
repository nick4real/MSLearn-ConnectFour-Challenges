window.playSound = (id) => {
    document.getElementById(id).currentTime = 0;
    document.getElementById(id).play();
}

window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text).catch((err) => {
        alert(err.message);
    });
}