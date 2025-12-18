(() => {
    const shell = document.querySelector(".adm-shell");
    const btn = document.getElementById("admSidebarToggle");

    if (!shell || !btn) return;

    btn.addEventListener("click", () => {
        const collapsed = shell.classList.toggle("is-collapsed");
        btn.setAttribute("aria-expanded", (!collapsed).toString());
    });
})();
