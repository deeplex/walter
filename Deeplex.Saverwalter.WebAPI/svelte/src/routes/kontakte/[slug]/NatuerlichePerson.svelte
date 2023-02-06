<script lang="ts">
	import { TextInput } from 'carbon-components-svelte';
	import type { NatuerlichePersonEntry } from './classes';

	export let person: NatuerlichePersonEntry;
</script>

<div>
	<TextInput labelText="Vorname" value={person.vorname} />
	<TextInput labelText="Nachname" value={person.nachname} />
</div>
