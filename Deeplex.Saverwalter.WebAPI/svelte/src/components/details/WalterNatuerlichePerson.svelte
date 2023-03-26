<script lang="ts">
	import {
		WalterMultiSelect,
		WalterPerson,
		WalterTextArea,
		WalterTextInput
	} from '$WalterComponents';
	import type {
		WalterNatuerlichePersonEntry,
		WalterSelectionEntry
	} from '$WalterLib';
	import { Row } from 'carbon-components-svelte';

	export let a: Partial<WalterNatuerlichePersonEntry>;
	export let juristischePersonen: WalterSelectionEntry[];
</script>

<Row>
	<WalterTextInput bind:value={a.vorname} labelText="Vorname" />
	<WalterTextInput bind:value={a.nachname} labelText="Nachname" />
</Row>
<WalterPerson value={a} />
<Row>
	<WalterMultiSelect
		bind:a={juristischePersonen}
		titleText="Juristische Personen"
		bind:value={a.selectedJuristischePersonen}
	/>
</Row>
<Row>
	<WalterTextArea bind:value={a.notiz} labelText="Notiz" />
</Row>
