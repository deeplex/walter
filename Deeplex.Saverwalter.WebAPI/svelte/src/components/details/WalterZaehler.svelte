<script lang="ts">
	import { WalterAdresse, WalterTextInput } from '$WalterComponents';
	import type { WalterZaehlerEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';

	export let a: Promise<WalterZaehlerEntry> | undefined = undefined;
	export let entry: Partial<WalterZaehlerEntry> = {};
</script>

<Row>
	<WalterTextInput
		bind:binding={entry.kennnummer}
		labelText="Kennnummer"
		value={a?.then((x) => x.kennnummer)}
	/>
	<!-- TODO -->
	<WalterTextInput labelText="Typ" value={a?.then((x) => x.typ)} />
</Row>
<WalterAdresse bind:entry={entry.adresse} adresse={a?.then((x) => x.adresse)} />
<Row>
	<WalterTextInput
		bind:binding={entry.notiz}
		labelText="Notiz"
		value={a?.then((x) => x.notiz)}
	/>
</Row>
