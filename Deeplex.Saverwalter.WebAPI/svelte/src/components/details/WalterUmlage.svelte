<script lang="ts">
	import { WalterTextInput } from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterUmlageEntry } from '$WalterTypes';

	export let a: Promise<WalterUmlageEntry> | undefined = undefined;
	export let entry: Partial<WalterUmlageEntry> = {};
</script>

<Row>
	<!-- TODO: THESE ARE NO TEXT INPUTS, BUT MENUS... PLEASE CHANGE -->
	<WalterTextInput labelText="Typ" value={a?.then((x) => x.typ)} />
	<WalterTextInput
		labelText="Wohnungen"
		value={a?.then((x) => x.wohnungenBezeichnung)}
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x.notiz)}
	/>
</Row>
