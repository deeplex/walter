<script lang="ts">
	import { TextInput } from 'carbon-components-svelte';
	import type { JuristischePersonEntry } from './classes';

	export let person: JuristischePersonEntry;
</script>

<TextInput labelText="Bezeichnung" value={person.name} />
