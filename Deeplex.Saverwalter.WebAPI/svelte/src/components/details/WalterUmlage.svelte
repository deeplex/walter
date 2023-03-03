<script lang="ts">
	import { WalterTextInput } from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterUmlageEntry } from '$WalterTypes';
	import WalterComboBox from '../elements/WalterComboBox.svelte';

	export let a: Promise<WalterUmlageEntry> | undefined = undefined;
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
	<!-- TODO REPLACE WITH LIST -->
	<!-- <WalterTextInput
		labelText="Wohnungen"
		value={a?.then((x) => x.wohnungenBezeichnung)}
	/> -->
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x.notiz)}
	/>
</Row>
