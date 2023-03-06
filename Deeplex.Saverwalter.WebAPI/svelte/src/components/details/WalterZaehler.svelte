<script lang="ts">
	import {
		WalterAdresse,
		WalterTextArea,
		WalterTextInput
	} from '$WalterComponents';
	import type { WalterSelectionEntry, WalterZaehlerEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';
	import WalterComboBox from '../elements/WalterComboBox.svelte';

	export let a: Partial<WalterZaehlerEntry> = {};
	export let zaehlertypen: WalterSelectionEntry[];
	export let wohnungen: WalterSelectionEntry[];
	export let zaehler: WalterSelectionEntry[];
</script>

<Row>
	<WalterTextInput bind:value={a.kennnummer} labelText="Kennnummer" />
	<WalterComboBox bind:value={a.typ} titleText="Typ" a={zaehlertypen} />
</Row>
<WalterAdresse bind:value={a.adresse} />
<Row>
	<WalterComboBox bind:value={a.wohnung} titleText="Wohnung" a={wohnungen} />
	<WalterComboBox
		bind:value={a.allgemeinZaehler}
		titleText="Allgemeinzähler"
		a={zaehler}
	/>
</Row>
<Row>
	<WalterTextArea bind:value={a.notiz} labelText="Notiz" />
</Row>
