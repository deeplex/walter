<script lang="ts">
	import {
		WalterMultiSelect,
		WalterPerson,
		WalterTextArea,
		WalterTextInput
	} from '$WalterComponents';
	import type {
		WalterJuristischePersonEntry,
		WalterSelectionEntry
	} from '$WalterLib';
	import { Row } from 'carbon-components-svelte';

	export let a: Partial<WalterJuristischePersonEntry>;
	export let juristischePersonen: WalterSelectionEntry[];
	export let kontakte: WalterSelectionEntry[];
</script>

<Row>
	<WalterTextInput labelText="Bezeichnung" value={a.name} />
</Row>
<WalterPerson value={a} />
<Row>
	<WalterMultiSelect
		bind:a={juristischePersonen}
		titleText="Juristische Personen"
		bind:value={a.selectedJuristischePersonen}
	/>
	<WalterMultiSelect
		titleText="Mitglieder"
		a={kontakte}
		bind:value={a.selectedMitglieder}
	/>
</Row>
<Row>
	<WalterTextArea bind:value={a.notiz} labelText="Notiz" />
</Row>
