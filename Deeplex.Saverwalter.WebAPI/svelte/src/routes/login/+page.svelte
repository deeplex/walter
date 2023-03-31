<script lang="ts">
	import { WalterHeader } from '$WalterComponents';
	import { walter_post } from '$WalterServices/requests';
	import Cookies from 'js-cookie';

	import {
		Button,
		Content,
		FluidForm,
		PasswordInput,
		TextInput
	} from 'carbon-components-svelte';
	import { Login } from 'carbon-icons-svelte';

	const login = {
		username: '',
		password: ''
	};

	let invalid = false;

	async function submit() {
		const response = await walter_post('/api/login', login);
		invalid = !response.succeeded;
		if (response.succeeded) {
			Cookies.set('access_token', response.accessToken, {
				path: '/',
				sameSite: 'strict',
				secure: import.meta.env.PROD,
				expires: 7 // days
			});
		} else {
			invalid = true;
		}
	}
</script>

<Content>
	<WalterHeader title="Anmeldeseite" />
	<FluidForm style="text-align: center; margin-top: 40vh">
		<TextInput
			bind:value={login.username}
			labelText="Nutzername"
			bind:invalid
			invalidText="Nutzername oder Passwort falsch"
			placeholder="Nutzername eintragen..."
			required
		/>
		<PasswordInput
			bind:value={login.password}
			bind:invalid
			invalidText="Nutzername oder Passwort falsch"
			required
			type="password"
			labelText="Passwort"
			placeholder="Passwort eintragen..."
		/>
		<Button on:click={submit} icon={Login}>Anmelden</Button>
	</FluidForm>
</Content>
