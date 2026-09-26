(function () {
    const toggleBtn = document.getElementById("chat-toggle");
    const box = document.getElementById("chat-box");
    const closeBtn = document.getElementById("chat-close");
    const form = document.getElementById("chat-form");
    const input = document.getElementById("chat-input");
    const list = document.getElementById("chat-messages");
    const sendBtn = form.querySelector("button");

    // المحادثة كلها بتتخزن هنا عشان البوت يفتكر اللي فات
    const history = [];

    function addMessage(role, text) {
        const div = document.createElement("div");
        div.className = "chat-msg " + (role === "user" ? "user" : "bot");
        div.dir = "auto";          // يظبط اتجاه العربي والإنجليزي تلقائيًا
        div.textContent = text;    // textContent مش innerHTML (أمان)
        list.appendChild(div);
        list.scrollTop = list.scrollHeight;
        return div;
    }

    toggleBtn.addEventListener("click", function () {
        box.hidden = !box.hidden;
        if (!box.hidden) {
            if (list.children.length === 0) {
                addMessage("assistant", "أهلًا بيك! 👋 أقدر أساعدك إزاي؟");
            }
            input.focus();
        }
    });

    closeBtn.addEventListener("click", function () {
        box.hidden = true;
    });

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const text = input.value.trim();
        if (!text) return;

        input.value = "";
        addMessage("user", text);
        history.push({ role: "user", content: text });

        const typing = addMessage("assistant", "...");
        sendBtn.disabled = true;

        try {
            const response = await fetch("/Chat/Send", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ messages: history })
            });

            if (!response.ok) throw new Error("HTTP " + response.status);

            const data = await response.json();
            typing.textContent = data.reply;
            history.push({ role: "assistant", content: data.reply });
        } catch (err) {
            typing.textContent = "حصلت مشكلة في الاتصال، جرّب تاني.";
            console.error(err);
        } finally {
            sendBtn.disabled = false;
            input.focus();
        }
    });
})();