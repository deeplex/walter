<script lang="ts">
	import {
		WalterComboBox,
		WalterDatePicker,
		WalterNumberInput,
		WalterTextArea,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type {
		WalterErhaltungsaufwendungEntry,
		WalterSelectionEntry
	} from '$WalterLib';

	export let kontakte: WalterSelectionEntry[];
	export let wohnungen: WalterSelectionEntry[];

	export let a: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<Row>
	<WalterTextInput bind:value={a.bezeichnung} labelText="Bezeichnung" />
	<WalterComboBox
		bind:value={a.aussteller}
		titleText="Aussteller"
		a={kontakte}
	/>
	<WalterDatePicker bind:value={a.datum} labelText="Datum" />
</Row>
<Row>
	<WalterComboBox bind:value={a.wohnung} titleText="Wohnung" a={wohnungen} />
	<WalterNumberInput bind:value={a.betrag} label="Betrag" />
</Row>
<Row>
	<WalterTextArea bind:value={a.notiz} labelText="Notiz" />
</Row>
