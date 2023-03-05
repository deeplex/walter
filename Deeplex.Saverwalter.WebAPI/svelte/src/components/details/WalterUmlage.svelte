<script lang="ts">
	import {
		WalterComboBox,
		WalterMultiSelect,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterUmlageEntry } from '$WalterTypes';

	export let a: Partial<WalterUmlageEntry> = {};
</script>

<Row>
	<WalterComboBox
		api={'/api/selection/betriebskostentypen'}
		bind:value={a.typ}
		titleText="Typ"
	/>
	<WalterComboBox
		api={'/api/selection/umlageschluessel'}
		bind:value={a.schluessel}
		titleText="Umlageschlüssel"
	/>
</Row>
<Row>
	<WalterMultiSelect
		bind:value={a.selectedWohnungen}
		api="/api/selection/wohnungen"
		titleText="Wohnungen"
	/>
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
