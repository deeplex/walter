<script lang="ts">
	import {
		WalterComboBox,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterUmlageEntry } from '$WalterTypes';

	export let a: Promise<Partial<WalterUmlageEntry>> | undefined = undefined;
	export let entry: Partial<WalterUmlageEntry> = {};
</script>

<Row>
	<WalterComboBox
		api={'/api/selection/betriebskostentypen'}
		bind:binding={entry.typ}
		titleText="Typ"
		value={a?.then((x) => x.typ)}
	/>
	<WalterComboBox
		api={'/api/selection/umlageschluessel'}
		bind:binding={entry.schluessel}
		titleText="Umlageschlüssel"
		value={a?.then((x) => x.schluessel)}
	/>
</Row>
<Row>
	<WalterMultiSelect
		value={a?.then((x) => x.selectedWohnungen)}
		bind:binding={entry.selectedWohnungen}
		api="/api/selection/wohnungen"
		titleText="Wohnungen"
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x.notiz)}
	/>
</Row>
