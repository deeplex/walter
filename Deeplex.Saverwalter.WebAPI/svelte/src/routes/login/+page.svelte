<script lang="ts">
	import { WalterHeader } from '$WalterComponents';

	import {
		Button,
		Content,
		FluidForm,
		PasswordInput,
		TextInput
	} from 'carbon-components-svelte';
	import { Login } from 'carbon-icons-svelte';
	import { goto } from '$app/navigation';
	import { WalterToastContent } from '$WalterLib';
	import { walter_sign_in } from '$WalterServices/auth';

	const login = {
		username: '',
		password: ''
	};

	let invalid = false;

	const LoginToast = new WalterToastContent(
		'Anmeldung erfolgreich',
		'Anmeldung fehlgeschlagen',
		() => `Anmeldung für Nutzer ${login.username} erfolgreich.`,
		() => 'Nutzername oder Passwort falsch.'
	);

	async function submit() {
		const response = await walter_sign_in(fetch, login.username, login.password, LoginToast);	
		if (response == null) {
			invalid = true;
		} else {
			goto('/');
		}
	}

	function handleEnterKey(event: KeyboardEvent) {
		if (event.key === 'Enter') {
			submit();
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
			on:keydown={handleEnterKey}
		/>
		<Button on:click={submit} icon={Login}>Anmelden</Button>
	</FluidForm>
</Content>
