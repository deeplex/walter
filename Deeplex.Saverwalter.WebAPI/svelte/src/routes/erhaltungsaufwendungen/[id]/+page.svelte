<script lang="ts">
	import type { PageData } from './$types';

	import type { ErhaltungsaufwendungEntry } from '$types';
	import { walter_get } from '$services/utils';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterErhaltungsaufwendung
	} from '$components';

	export let data: PageData;
	const url = `/api/erhaltungsaufwendungen/${data.id}`;

	const a: Promise<ErhaltungsaufwendungEntry> = walter_get(url);
	const entry: Partial<ErhaltungsaufwendungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.aussteller.text + ' - ' + x.bezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterErhaltungsaufwendung {a} {entry} />
</WalterGrid>
