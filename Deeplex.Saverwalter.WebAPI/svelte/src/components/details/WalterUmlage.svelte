<script lang="ts">
	import {
		WalterComboBox,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterSelectionEntry, WalterUmlageEntry } from '$WalterTypes';

	export let betriebskostentypen: WalterSelectionEntry[];
	export let umlageschluessel: WalterSelectionEntry[];
	export let wohnungen: WalterSelectionEntry[];

	export let a: Partial<WalterUmlageEntry> = {};
</script>

<Row>
	<WalterComboBox a={betriebskostentypen} bind:value={a.typ} titleText="Typ" />
	<WalterComboBox
		a={umlageschluessel}
		bind:value={a.schluessel}
		titleText="Umlageschlüssel"
	/>
</Row>
<Row>
	<WalterMultiSelect
		bind:value={a.selectedWohnungen}
		a={wohnungen}
		titleText="Wohnungen"
	/>
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
