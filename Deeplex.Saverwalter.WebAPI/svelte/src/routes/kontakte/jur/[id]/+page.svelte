<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { JuristischePersonEntry } from '../../../../types/juristischeperson.type';
	import { walter_get } from '../../../../services/utils';
	import {
		Person,
		WalterHeader,
		WalterGrid,
		WalterTextInput
	} from '../../../../components';

	export let data: PageData;
	const a: Promise<JuristischePersonEntry> = walter_get(
		`/api/kontakte/jur/${data.id}`
	);
</script>

<WalterHeader title={a.then((e) => e.name)} />

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Bezeichnung" value={a.then((x) => x.name)} />
	</Row>
	<Person person={a} />
</WalterGrid>
