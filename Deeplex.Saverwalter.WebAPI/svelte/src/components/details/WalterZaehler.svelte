<script lang="ts">
	import { Adresse, WalterTextInput } from '$components';
	import type { ZaehlerEntry } from '$types';
	import { Row } from 'carbon-components-svelte';

	export let a: Promise<ZaehlerEntry> | undefined = undefined;
	export let entry: Partial<ZaehlerEntry> = {};
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
<!-- TODO -->
<Adresse adresse={a?.then((x) => x.adresse)} />
<Row>
	<WalterTextInput
		bind:binding={entry.notiz}
		labelText="Notiz"
		value={a?.then((x) => x.notiz)}
	/>
</Row>
