import { addToast } from '$WalterStore';
import type { WalterS3File } from '$WalterTypes';
import * as parser from 'fast-xml-parser';
import { walter_delete, walter_fetch } from './requests';
import type { WalterToastContent } from '../lib/WalterToastContent';

const baseURL = '/api/files';
type XMLResult = {
  ListBucketResult?: { Contents?: WalterS3File | WalterS3File[] };
};

export const walter_s3_post = (
  file: File,
  path: string,
  f: typeof fetch,
  toast?: WalterToastContent
) =>
  walter_fetch(f, `${baseURL}/${path}/${file.name}`, {
    method: 'PUT',
    headers: {
      // Ignored, due to header being replaced in walter_fetch
      'Content-Type': `${file.type}`
    },
    body: file
  }).then((e) => finish_s3_post(e, toast));

export async function finish_s3_post(e: Response, toast?: WalterToastContent) {
  toast && addToast(toast, e.status === 200);
  return e;
}

export const walter_s3_get = (S3URL: string): Promise<any> =>
  walter_fetch(fetch, `${baseURL}/${S3URL}`, { method: 'GET' }).then((e) =>
    e.blob()
  );

export function walter_s3_delete(file: WalterS3File) {
  return walter_delete(`${baseURL}/${file.Key}`);
}

export function download_file_blob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

export function walter_s3_get_files(S3prefixURL: string, f: typeof fetch) {
  const url = `${baseURL}?prefix=${S3prefixURL}`;
  const requestInit = { method: 'GET' };

  return walter_fetch(f, url, requestInit)
    .then((e) => e.body?.getReader().read())
    .then(parse_stream_into_walter_s3_files);
}

function parse_stream_into_walter_s3_files(
  e: ReadableStreamReadResult<Uint8Array> | undefined
) {
  if (!e) {
    return [];
  }
  const result: XMLResult = new parser.XMLParser().parse(
    new TextDecoder().decode(e.value)
  );

  const Contents = result?.ListBucketResult?.Contents;

  if (!Contents) {
    return [];
  } else if (Array.isArray(Contents)) {
    return Contents.map(create_walter_s3_file_from_xml_parse) || [];
  } else {
    return [create_walter_s3_file_from_xml_parse(Contents)];
  }
}

export function create_walter_s3_file_from_file(
  file: File,
  S3URL: string
): WalterS3File {
  return {
    FileName: file.name,
    Key: `${S3URL}/${file.name}`,
    LastModified: file.lastModified,
    Type: file.type,
    Size: file.size,
    Blob: file
  };
}

function create_walter_s3_file_from_xml_parse(e: WalterS3File): WalterS3File {
  return {
    FileName: e.Key.split('/').pop()!,
    Key: e.Key,
    LastModified: e.LastModified,
    Size: e.Size
  };
}