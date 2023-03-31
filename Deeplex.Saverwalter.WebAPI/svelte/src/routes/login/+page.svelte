<script lang="ts">
	import { WalterHeader } from '$WalterComponents';
	import {
		Content,
		FluidForm,
		PasswordInput,
		TextInput
	} from 'carbon-components-svelte';
	// import 'carbon-components-svelte/css/white.css';
	import 'carbon-components-svelte/css/all.css';
</script>

<Content>
	<FluidForm style="text-align: center; margin-top: 40vh">
		<TextInput
			labelText="Nutzername"
			placeholder="Nutzername eintragen..."
			required
		/>
		<PasswordInput
			required
			type="password"
			labelText="Passwort"
			placeholder="Passwort eintragen..."
		/>
	</FluidForm>
</Content>
