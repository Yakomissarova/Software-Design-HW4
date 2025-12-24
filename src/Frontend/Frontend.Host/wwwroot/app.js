const els = {
    login: document.getElementById("login"),
    btnApplyLogin: document.getElementById("btnApplyLogin"),
    btnRefresh: document.getElementById("btnRefresh"),

    curLogin: document.getElementById("curLogin"),
    curUserId: document.getElementById("curUserId"),
    curBalance: document.getElementById("curBalance"),

    mainOk: document.getElementById("mainOk"),
    mainErr: document.getElementById("mainErr"),

    topupAmount: document.getElementById("topupAmount"),
    btnTopup: document.getElementById("btnTopup"),
    topupOk: document.getElementById("topupOk"),
    topupErr: document.getElementById("topupErr"),

    orderAmount: document.getElementById("orderAmount"),
    orderDesc: document.getElementById("orderDesc"),
    btnCreateOrder: document.getElementById("btnCreateOrder"),
    ordOk: document.getElementById("ordOk"),
    ordErr: document.getElementById("ordErr"),

    btnLoadOrders: document.getElementById("btnLoadOrders"),
    ordersBody: document.getElementById("ordersBody"),
    listErr: document.getElementById("listErr"),
};

let state = {
    login: "",
    userId: "",
    balance: null
};

function setOk(el, msg) {
    el.textContent = msg || "";
}
function setErr(el, msg) {
    el.textContent = msg || "";
}

function setMainOk(msg) { setOk(els.mainOk, msg); }
function setMainErr(msg) { setErr(els.mainErr, msg); }

function cleanLogin(s) {
    return (s || "").trim();
}

async function httpJson(method, url, bodyObj) {
    const opts = { method, headers: { "Content-Type": "application/json" } };
    if (bodyObj !== undefined) opts.body = JSON.stringify(bodyObj);

    const resp = await fetch(url, opts);
    const text = await resp.text();

    // если ответ пустой
    let data = null;
    if (text && text.length > 0) {
        try { data = JSON.parse(text); }
        catch { data = text; }
    }
    return { ok: resp.ok, status: resp.status, data };
}

function renderHeader() {
    els.curLogin.textContent = state.login || "—";
    els.curUserId.textContent = state.userId || "—";
    els.curBalance.textContent = (state.balance === null || state.balance === undefined) ? "—" : String(state.balance);
}

function renderOrders(items) {
    els.ordersBody.innerHTML = "";
    if (!items || items.length === 0) {
        els.ordersBody.innerHTML = `<tr><td colspan="3" class="muted">Пока пусто</td></tr>`;
        return;
    }

    for (const o of items) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
      <td>${escapeHtml(o.publicId ?? o.PublicId ?? "")}</td>
      <td>${escapeHtml(String(o.amount ?? o.Amount ?? ""))}</td>
      <td>${escapeHtml(o.status ?? o.Status ?? "")}</td>
    `;
        els.ordersBody.appendChild(tr);
    }
}

function escapeHtml(s) {
    return String(s)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

/**
 * Создать аккаунт по логину.
 * Если уже есть -> сервер вернёт 409 Conflict.
 * Мы это считаем нормальным и просто идём получать аккаунт.
 */
async function ensureAccount(login) {
    // Пытаемся создать
    const create = await httpJson("POST", "/api/accounts", { login });

    if (create.ok) return true;

    // 409 значит уже есть — это нормально
    if (create.status === 409) return true;

    // Иначе ошибка
    throw new Error(`Не удалось создать аккаунт (${create.status}): ${stringifyErr(create.data)}`);
}

async function loadAccount(login) {
    const resp = await httpJson("GET", `/api/accounts/by-login/${encodeURIComponent(login)}`);
    if (!resp.ok) throw new Error(`Аккаунт не найден/ошибка (${resp.status}): ${stringifyErr(resp.data)}`);

    // AccountResponse: { userId, login, balance }
    const acc = resp.data;
    state.userId = acc.userId ?? acc.UserId ?? "";
    state.balance = acc.balance ?? acc.Balance ?? 0;
    renderHeader();
}

async function loadOrders(login) {
    const resp = await httpJson("GET", `/api/orders?login=${encodeURIComponent(login)}`);
    if (!resp.ok) throw new Error(`Не удалось получить заказы (${resp.status}): ${stringifyErr(resp.data)}`);

    renderOrders(resp.data);
}

async function refreshAll() {
    setMainOk("");
    setMainErr("");
    setOk(els.topupOk, "");
    setErr(els.topupErr, "");
    setOk(els.ordOk, "");
    setErr(els.ordErr, "");
    setErr(els.listErr, "");

    if (!state.login) {
        renderHeader();
        renderOrders([]);
        return;
    }

    await loadAccount(state.login);
    await loadOrders(state.login);
}

function stringifyErr(e) {
    if (e === null || e === undefined) return "";
    if (typeof e === "string") return e;
    try { return JSON.stringify(e); } catch { return String(e); }
}

async function applyLoginFlow() {
    const login = cleanLogin(els.login.value);
    if (!login) {
        setMainErr("Введите логин.");
        return;
    }

    state.login = login;
    state.userId = "";
    state.balance = null;
    renderHeader();

    setMainOk("");
    setMainErr("");

    try {
        // 1) создать/убедиться что аккаунт есть
        await ensureAccount(login);

        // 2) получить аккаунт (баланс+userId)
        await loadAccount(login);

        // 3) получить заказы
        await loadOrders(login);

        setMainOk(`ОК. Аккаунт готов. Login: ${login}`);
    } catch (err) {
        setMainErr(String(err?.message ?? err));
    }
}

async function topupFlow() {
    setOk(els.topupOk, "");
    setErr(els.topupErr, "");

    if (!state.login) {
        setErr(els.topupErr, "Сначала введи логин и нажми 'Войти / создать аккаунт'.");
        return;
    }

    const amount = Number(els.topupAmount.value);
    if (!Number.isFinite(amount) || amount <= 0) {
        setErr(els.topupErr, "Введите сумму > 0.");
        return;
    }

    try {
        const resp = await httpJson("POST", "/api/accounts/topup-by-login", { login: state.login, amount });
        if (!resp.ok) throw new Error(`Ошибка пополнения (${resp.status}): ${stringifyErr(resp.data)}`);

        // Обновляем баланс после пополнения
        await loadAccount(state.login);

        setOk(els.topupOk, `Баланс пополнен на ${amount}. Текущий баланс: ${state.balance}`);
    } catch (err) {
        setErr(els.topupErr, String(err?.message ?? err));
    }
}

async function createOrderFlow() {
    setOk(els.ordOk, "");
    setErr(els.ordErr, "");

    if (!state.login) {
        setErr(els.ordErr, "Сначала введи логин и нажми 'Войти / создать аккаунт'.");
        return;
    }

    const amount = Number(els.orderAmount.value);
    const description = (els.orderDesc.value || "").trim();

    if (!Number.isFinite(amount) || amount <= 0) {
        setErr(els.ordErr, "Введите сумму заказа > 0.");
        return;
    }
    if (!description) {
        setErr(els.ordErr, "Введите описание заказа.");
        return;
    }

    try {
        const resp = await httpJson("POST", "/api/orders", { login: state.login, amount, description });
        if (!resp.ok) throw new Error(`Ошибка создания заказа (${resp.status}): ${stringifyErr(resp.data)}`);

        const publicId = resp.data?.publicId ?? resp.data?.PublicId ?? "(unknown)";
        setOk(els.ordOk, `Заказ создан: ${publicId}`);

        // Обновим список заказов
        await loadOrders(state.login);
    } catch (err) {
        setErr(els.ordErr, String(err?.message ?? err));
    }
}

// кнопки
els.btnApplyLogin.addEventListener("click", applyLoginFlow);
els.btnRefresh.addEventListener("click", () => refreshAll().catch(e => setMainErr(String(e?.message ?? e))));
els.btnTopup.addEventListener("click", topupFlow);
els.btnCreateOrder.addEventListener("click", createOrderFlow);
els.btnLoadOrders.addEventListener("click", async () => {
    setErr(els.listErr, "");
    if (!state.login) {
        setErr(els.listErr, "Сначала введи логин и нажми 'Войти / создать аккаунт'.");
        return;
    }
    try {
        await loadOrders(state.login);
    } catch (e) {
        setErr(els.listErr, String(e?.message ?? e));
    }
});

// Enter в поле логина
els.login.addEventListener("keydown", (e) => {
    if (e.key === "Enter") applyLoginFlow();
});

// при загрузке страницы — дефолт
renderHeader();
renderOrders([]);
