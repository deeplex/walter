<script lang="ts">
	import { WalterTextInput } from '$components';
	import { Row } from 'carbon-components-svelte';
	import type { UmlageEntry } from '$types';

	export let a: Promise<UmlageEntry> | undefined = undefined;
	export let entry: Partial<UmlageEntry> = {};
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
