window.playSound = (id) => {
    document.getElementById(id).currentTime = 0;
    document.getElementById(id).play();
}