window.auth = {
	login: async function (role) {
		await fetch("/api/login", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ role })
		});

		location.reload();
	},

	logout: async function () {
		await fetch("/api/logout", { method: "POST" });
		location.reload();
	},


	aloginOLD: async function (username, password) {
		const response = await fetch("/api/Userlogin", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ username, password })
		});

		// ✅ Maintenance / forbidden
		if (response.status === 403) {
			const problem = await response.json();
			console.log("Login failed:", problem);
			throw new Error(problem.detail || "Login is temporarily disabled.");
		}
		// ✅ Invalid credentials
		if (response.status === 401) {
			throw new Error("Invalid username or password.");
		}

		// ✅ Unexpected error
		if (!response.ok) {
			throw new Error("DK - Invalid credentials");
		}

		// ✅ Success
		location.href = "/";
	},

	alogin: async function (username, password) {

		const response = await fetch("/api/auth/login", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ username, password })
		});

		if (response.status === 403) {
			console.log("Login failed:", response.status);
			return { success: false, reason: "maintenance" };
		}

		if (response.status === 401) {
			console.log("Login failed:", response.status);
			return { success: false, reason: "invalid" };
		}

		if (!response.ok) {
			console.log("Login failed:", response.status);
			return { success: false, reason: "error" };
		}

		return { success: true };
	},
    
    alogout: async function () {
		await fetch("/api/auth/logout", { method: "POST" });
		location.href = "/";
	}

};